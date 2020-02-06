using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Traversal {
    public class NoTraversePathAttribute : Attribute { }

    public static class Traversing {
        public static void Traverse(Expr node) {
            Action<ITreePath> walker = Walker;
            if (!node.Path.Traversed) {
                walker(node.Path);
            }

            node = node.Path.Node;
            PropertyInfo[] exprProps = node.GetType().GetProperties();
            IEnumerable<PropertyInfo> childProps = exprProps.Where(
                p => typeof(Expr).IsAssignableFrom(p.PropertyType)
                  && !Attribute.IsDefined(p, typeof(NoTraversePathAttribute), false)
                  || p.PropertyType.IsGenericType
                  && p.PropertyType
                      .GetInterfaces()
                      .Where(i => i.IsGenericType)
                      .Select(i => i.GetGenericTypeDefinition())
                      .Contains(typeof(IList<>))
                  && typeof(Span).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]));
            foreach (PropertyInfo prop in childProps) {
                object obj = prop.GetValue(node);
                if (obj == null) {
                    continue;
                }

                if (obj is Expr expr) {
                    Traverse(expr);
                }
                else {
                    try {
                        List<Span> list = ((IEnumerable) obj).OfType<Span>().ToList();
                        // for loop required, expressions collection
                        // can be modified.
                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (var i = 0; i < list.Count; i++) {
                            Traverse((Expr) list[i]);
                        }
                    }
                    catch (InvalidCastException) {
                        // ignored
                    }
                }
            }
        }

        public static void Walker(ITreePath path) {
            switch (path.Node) {
            case TupleTypeName t when t.Types.Count == 0: {
                path.Node      = new SimpleTypeName("UnitType");
                path.Traversed = true;
                break;
            }

            case UnionTypeName unionTypeName: {
                // LeftType | RightType -> Union[LeftType, RightType]
                path.Node = new GenericTypeName(
                    path.Node.Parent,
                    new SimpleTypeName("Union"),
                    new NodeList<TypeName>(path.Node) {
                        unionTypeName.Left,
                        unionTypeName.Right
                    }
                );
                path.Traversed = true;
                break;
            }

            case BinaryExpr bin when bin.Operator.Is(TokenType.OpIs)
                                  && bin.Right is UnaryExpr un
                                  && un.Operator.Is(TokenType.OpNot): {
                // x is (not (y)) -> not (x is y)
                path.Node = new UnaryExpr(
                    path.Node.Parent,
                    TokenType.OpNot,
                    new BinaryExpr(
                        path.Node,
                        bin.Left,
                        new OperatorToken(path.Node.Source, tokenType: TokenType.OpIs),
                        un.Value
                    )
                );
                path.Traversed = true;
                break;
            }

            case BinaryExpr bin when bin.Operator.Is(TokenType.RightPipeline): {
                // arg |> func -> func(arg)
                path.Node = new FuncCallExpr(
                    path.Node.Parent,
                    bin.Right,
                    new FuncCallArg(path.Node.Parent, bin.Left)
                );
                path.Traversed = true;
                break;
            }

            case BinaryExpr bin when bin.Operator.Is(TokenType.OpAssign)
                                  && bin.Left is TupleExpr tpl: {
                // (x, y) = GetCoordinates()
                // <=======================>
                // unwrappedX = GetCoordinates()
                // x = unwrappedX.x
                // y = unwrappedX.y
                var block = bin.GetParentOfType<BlockExpr>();
                (_, int deconstructionIdx) = block.IndexOf(bin);
                var deconstructionVar = new VarDef(
                    block,
                    new NameExpr(block.CreateUniqueId("unwrapped{0}")),
                    value: bin.Right,
                    immutable: true
                );
                block.Items[deconstructionIdx] = deconstructionVar;
                for (var i = 0; i < tpl.Expressions.Count; i++) {
                    block.Items.Insert(
                        deconstructionIdx + i + 1,
                        new BinaryExpr(
                            block,
                            tpl.Expressions[i],
                            new OperatorToken(path.Node.Source,
                                              tokenType: TokenType.OpAssign),
                            new MemberAccessExpr(block, deconstructionVar.Name) {
                                Member = tpl.Expressions[i]
                            }
                        )
                    );
                }

                path.Traversed = true;
                break;
            }

            case WhileExpr whileExpr when whileExpr.NoBreakBlock != null: {
                // Add bool before loop, that indicates, was break reached or not.
                // Find all 'break'-s in child blocks and set this
                // bool to 'true' before exiting the loop.
                // Example:
                // while x
                //     do()
                //     if a
                //         do2()
                //         break
                // nobreak
                //     do3()
                // <============================>
                // loop_X_nobreak = true
                // while x
                //     do()
                //     if a
                //         do2()
                //         loop_X_nobreak = false
                //         break
                // if loop_X_nobreak
                //     do3()
                var block = path.Node.GetParentOfType<BlockExpr>();
                (_, int whileIndex) = block.IndexOf(whileExpr);
                var flagName = new NameExpr(block.CreateUniqueId("loop_{0}_nobreak"));
                block.Items.Insert(
                    whileIndex,
                    new VarDef(
                        path.Node,
                        flagName,
                        value: new ConstantExpr(path.Node, "true")
                    )
                );
                // index of while == whileIdx + 1
                List<(BreakExpr item, BlockExpr itemParentBlock, int itemIndex)> breaks =
                    whileExpr.Block.FindItemsOfType<BreakExpr>();
                var boolSetter = new BinaryExpr(
                    path.Node,
                    flagName,
                    new OperatorToken(path.Node.Source, tokenType: TokenType.OpAssign),
                    new ConstantExpr(path.Node, "false")
                );
                foreach ((_, BlockExpr itemParentBlock, int itemIndex) in breaks) {
                    itemParentBlock.Items.Insert(itemIndex, boolSetter);
                }

                block.Items.Insert(
                    whileIndex + 2,
                    new ConditionalExpr(path.Node, flagName, whileExpr.NoBreakBlock)
                );
                whileExpr.NoBreakBlock = null;
                path.Traversed         = true;
                break;
            }
            }
        }
    }
}