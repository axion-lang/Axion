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
        public static void Traverse(Expr node, Action<ITreePath> walker) {
            if (!node.Path.Traversed) {
                walker(node.Path);
            }

            node = node.Path.Node;
            PropertyInfo[] exprProps = node.GetType().GetProperties();
            IEnumerable<PropertyInfo> childProps = exprProps.Where(
                p => typeof(Expr).IsAssignableFrom(p.PropertyType)
                  && !Attribute.IsDefined(p, typeof(NoTraversePathAttribute))
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
                    Traverse(expr, walker);
                }
                else {
                    try {
                        List<Span> list = ((IEnumerable) obj).OfType<Span>().ToList();
                        for (var i = 0; i < list.Count; i++) {
                            Traverse((Expr) list[i], walker);
                        }
                    }
                    catch (InvalidCastException) {
                        // ignored
                    }
                }
            }
        }

        public static void Walker(ITreePath path) {
            if (path.Node is UnionTypeName unionTypeName) {
                path.Node = new GenericTypeName(
                    path.Node.Parent,
                    new SimpleTypeName("Union"),
                    new NodeList<TypeName>(path.Node) {
                        unionTypeName.Left,
                        unionTypeName.Right
                    }
                );
                path.Traversed = true;
            }
            if (path.Node is BinaryExpr bin) {
                if (bin.Operator.Is(TokenType.OpIs)
                 && bin.Right is UnaryExpr un
                 && un.Operator.Is(TokenType.OpNot)) {
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
                }
                else if (bin.Operator.Is(TokenType.RightPipeline)) {
                    // arg |> func -> func(arg)
                    path.Node = new FuncCallExpr(
                        path.Node.Parent,
                        bin.Right,
                        new FuncCallArg(path.Node.Parent, bin.Left)
                    );
                    path.Traversed = true;
                }
            }
            else if (path.Node is WhileExpr whileExpr && whileExpr.NoBreakBlock != null) {
                // Add bool before loop, that indicates, was break reached or not.
                // Find all 'break's in child blocks and set this
                // bool to 'true' before exiting the loop.
                // Example:
                // while x
                //     do()
                //     if a
                //         do2()
                //         break
                // nobreak
                //     do3()
                // <=========================>
                // bool loop_X_nobreak = true;
                // while (x)
                // {
                //     do();
                //     if (a)
                //     {
                //         do2();
                //         loop_X_nobreak = false;
                //         break;
                //     }
                // }
                // if (loop_X_nobreak)
                // {
                //     do3()
                // }
                var block = path.Node.GetParentOfType<BlockExpr>();
                (BlockExpr whileParentBlock, int whileIndex) = block.IndexOf(whileExpr);
                var flagName = new NameExpr(block.CreateUniqueId("loop_{0}_nobreak"));
                whileParentBlock.Items.Insert(whileIndex, new VarDef(
                                                  path.Node,
                                                  flagName,
                                                  value: new ConstantExpr(path.Node, "true")
                                              ));
                // index of while == whileIdx + 1
                List<(BreakExpr item, BlockExpr itemParentBlock, int itemIndex)> breaks =
                    whileExpr.Block.FindItemsOfType<BreakExpr>();
                var boolSetter = new BinaryExpr(
                    path.Node,
                    flagName,
                    new OperatorToken(path.Node.Source, tokenType: TokenType.OpAssign),
                    new ConstantExpr(path.Node, "false")
                );
                foreach ((BreakExpr item, BlockExpr itemParentBlock, int itemIndex) brk in breaks) {
                    brk.itemParentBlock.Items.Insert(brk.itemIndex, boolSetter);
                }

                whileParentBlock.Items.Insert(
                    whileIndex + 2,
                    new ConditionalExpr(path.Node, flagName, whileExpr.NoBreakBlock)
                );
                whileExpr.NoBreakBlock = null;
                path.Traversed         = true;
            }
        }
    }
}