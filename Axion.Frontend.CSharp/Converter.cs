using System.Collections.Generic;
using System.Linq;
using System.Web;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Translation;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Frontend.CSharp {
    public class Translator : INodeTranslator {
        public string OutputFileExtension => ".cs";

        public bool Translate(CodeWriter w, ITranslatableNode node) {
            switch (node) {
            case CharToken e: {
                w.Write("'", HttpUtility.JavaScriptStringEncode(e.Content));
                if (!e.IsUnclosed)
                    w.Write("'");

                break;
            }
            case CommentToken e: {
                if (e.IsMultiline) {
                    w.Write("/*" + e.Content);
                    if (!e.IsUnclosed)
                        w.Write("*/");
                }
                else {
                    w.Write("//" + e.Content);
                }

                break;
            }
            case StringToken e: {
                if (e.HasPrefix("f"))
                    w.Write("$");

                w.Write("\"", HttpUtility.JavaScriptStringEncode(e.Content));
                if (!e.IsUnclosed)
                    w.Write("\"");

                break;
            }
            case NameExpr e: {
                if (e.IsSimple && e.Qualifiers[0].Content == "self") {
                    w.Write("this");
                    return true;
                }

                w.Write(string.Join("", e.Tokens.Select(t => t.Content)));
                break;
            }
            case YieldExpr e: {
                w.Write("yield return ", e.Value);
                break;
            }
            case ClassDef e: {
                w.Write("public class ", e.Name);
                if (e.Bases.Count > 0) {
                    w.Write(" : ");
                    w.AddJoin(", ", e.Bases);
                }

                w.WriteLine();
                w.Write(e.Scope);
                break;
            }
            case FunctionDef e: {
                if (e.Name == null) {
                    w.Write("(");
                    w.AddJoin(", ", e.Parameters);
                    w.Write(") => ", e.Scope);
                }
                else {
                    // BUG type inference stuck in infinite loop (get-value in TestMathExprParser)
                    w.Write(
                        "public ",
                        e.ValueType,
                        " ",
                        e.Name,
                        "("
                    );
                    w.AddJoin(", ", e.Parameters);
                    w.WriteLine(")");
                    w.Write(e.Scope);
                }

                break;
            }
            case ModuleDef e: {
                w.Write("namespace ", e.Name);
                w.WriteLine();
                w.Write(e.Scope);
                break;
            }
            case VarDef e: {
                if (e.Value == null) {
                    w.Write(e.ValueType, " ", e.Name);
                }
                else {
                    w.Write(
                        (object?) e.ValueType ?? "var",
                        " ",
                        e.Name,
                        " = ",
                        e.Value
                    );
                }

                break;
            }
            case FunctionParameter e: {
                if (!(e.Parent is FunctionDef f && f.Name == null)) {
                    w.Write(e.ValueType, " ");
                }

                w.Write(e.Name);
                if (e.Value != null) {
                    w.Write(" = ", e.Value);
                }

                break;
            }
            case NameDef e: {
                if (e.Value == null) {
                    w.Write(e.ValueType, " ", e.Name);
                }
                else {
                    w.Write(
                        (object?) e.ValueType ?? "var",
                        " ",
                        e.Name,
                        " = ",
                        e.Value
                    );
                }

                break;
            }
            case BinaryExpr e: {
                if (e.Operator!.Is(OpPower)) {
                    w.Write(
                        "Math.Pow(",
                        e.Left,
                        ", ",
                        e.Right,
                        ")"
                    );
                }
                else if (e.Operator.Is(OpIn)) {
                    // in (list1 or|and list2)
                    if (e.Right is ParenthesizedExpr paren
                     && paren.Value is BinaryExpr collections
                     && collections.Operator!.Is(OpAnd, OpOr)) {
                        w.Write(
                            collections.Right,
                            ".Contains(",
                            e.Left,
                            ") ",
                            Spec.CSharp.BinaryOperators[
                                collections.Operator.Type],
                            " ",
                            collections.Left,
                            ".Contains(",
                            e.Left,
                            ")"
                        );
                    }
                    else {
                        w.Write(
                            e.Right,
                            ".Contains(",
                            e.Left,
                            ")"
                        );
                    }
                }
                else {
                    if (!Spec.CSharp.BinaryOperators.TryGetValue(
                        e.Operator.Type,
                        out var op
                    )) {
                        op = e.Operator.Value;
                    }

                    w.Write(
                        e.Left,
                        " ",
                        op,
                        " ",
                        e.Right
                    );
                }

                break;
            }
            case TernaryExpr e: {
                w.Write(
                    e.Condition,
                    " ? ",
                    e.TrueExpr,
                    " : "
                );
                if (e.FalseExpr == null) {
                    w.Write("default");
                }
                else {
                    w.Write(e.FalseExpr);
                }

                break;
            }
            case UnaryExpr e: {
                var op = e.Operator.Value;
                if (op == "not") {
                    op = "!";
                }

                if (e.Operator.Side == InputSide.Right) {
                    w.Write(
                        op,
                        " (",
                        e.Value,
                        ")"
                    );
                }
                else {
                    w.Write(
                        "(",
                        e.Value,
                        ") ",
                        op
                    );
                }

                break;
            }
            case ForComprehension e: {
                w.Write(
                    "from ",
                    e.Item,
                    " in ",
                    e.Iterable
                );
                if (e.Right != null) {
                    w.Write(" ", e.Right);
                }

                if (e.Conditions.Count > 0) {
                    w.Write(" where ");
                    w.AddJoin(" && ", e.Conditions);
                }

                if (!e.IsNested) {
                    w.Write(" select ", e.Target);
                }

                break;
            }
            case FuncCallArg e: {
                if (e.Name != null) {
                    w.Write(e.Name, ": ");
                }

                w.Write(e.Value);
                break;
            }
            case EmptyExpr _: {
                // Don't write anything, semicolon is inserted at the scope level.
                break;
            }
            case WhileExpr e: {
                w.Write("while (", e.Condition);
                w.WriteLine(")");
                w.Write(e.Scope);
                break;
            }
            case FuncTypeName e: {
                w.Write(
                    "Func<",
                    e.ArgsType,
                    ", ",
                    e.ReturnType,
                    ">"
                );
                break;
            }
            case GenericTypeName e: {
                w.Write(e.Target, "<");
                w.AddJoin(",", e.TypeArgs);
                w.Write(">");
                break;
            }
            case Ast e: {
                if (e.Items.Count == 0) {
                    return true;
                }

                var defaultDirectives = new[] {
                    "System",
                    "System.IO",
                    "System.Linq",
                    "System.Text",
                    "System.Numerics",
                    "System.Threading",
                    "System.Diagnostics",
                    "System.Collections",
                    "System.Collections.Generic"
                };
                foreach (var directive in defaultDirectives) {
                    w.WriteLine($"using {directive};");
                }

                var rootItems     = new NodeList<Expr>(e);
                var rootClasses   = new List<ClassDef>();
                var rootFunctions = new List<FunctionDef>();
                foreach (var expr in e.Items) {
                    var item = expr;
                    // decorator is just a wrapper,
                    // so we need to unpack it's content.
                    if (expr is DecoratedExpr dec) {
                        item = dec.Target!;
                    }

                    switch (item) {
                    case ModuleDef m:
                        w.Write(m);
                        break;
                    case ClassDef c:
                        rootClasses.Add(c);
                        break;
                    case FunctionDef f:
                        rootFunctions.Add(f);
                        break;
                    default:
                        rootItems += expr;
                        break;
                    }
                }

                w.Write(
                    new ModuleDef(e) {
                        Name = new NameExpr(e, "__RootModule__")
                    }.WithScope(
                        new[] {
                            new ClassDef(e) {
                                Name = new NameExpr(e, "__RootClass__")
                            }.WithScope(
                                new[] {
                                    new FunctionDef(e) {
                                            Name = new NameExpr(e, "Main"),
                                            ValueType =
                                                new SimpleTypeName(e, "void")
                                        }.WithParameters(
                                             new FunctionParameter(e) {
                                                 Name = new NameExpr(e, "args"),
                                                 ValueType =
                                                     new ArrayTypeName(e) {
                                                         ElementType =
                                                             new SimpleTypeName(
                                                                 e,
                                                                 Spec.StringType
                                                             )
                                                     }
                                             }
                                         )
                                         .WithScope(rootItems)
                                         .WithDecorators(
                                             new NameExpr(e, "static")
                                         )
                                }.Union<Expr>(rootFunctions)
                            )
                        }.Union(rootClasses)
                    )
                );
                break;
            }
            case DecoratedExpr e: {
                foreach (var decorator in e.Decorators) {
                    if (decorator is NameExpr n
                     && Spec.CSharp.AllowedModifiers.Contains(n.ToString())) {
                        w.Write(n, " ");
                    }
                }

                w.Write(e.Target);
                break;
            }
            case IfExpr e: {
                w.Write("if (", e.Condition);
                w.WriteLine(")");
                w.Write(e.ThenScope);
                if (e.ElseScope == null) {
                    return true;
                }

                w.WriteLine("else");
                w.Write(e.ElseScope);
                break;
            }
            case ScopeExpr e: {
                w.WriteLine("{");
                w.IndentLevel++;
                foreach (var item in e.Items) {
                    w.Write(item);
                    if (!(e.Parent is ClassDef || e.Parent is ModuleDef)
                     && !(item is IDefinitionExpr
                       || item is IfExpr
                       || item is WhileExpr
                       || item is MacroApplicationExpr)
                     || item is VarDef) {
                        w.Write(";");
                    }

                    w.MaybeWriteLine();
                }

                w.IndentLevel--;
                w.Write("}");
                w.MaybeWriteLine();
                break;
            }
            default: {
                return false;
            }
            }

            return true;
        }
    }
}
