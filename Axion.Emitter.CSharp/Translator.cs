using System.Collections.Generic;
using System.Linq;
using System.Web;
using Axion.Core.Processing;
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
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Emitter.CSharp {
    public class Translator : INodeTranslator {
        public string OutputFileExtension => ".cs";

        public bool Translate(CodeWriter w, ITranslatableNode node) {
            switch (node) {
            case CharToken e: {
                w.Write("'", HttpUtility.JavaScriptStringEncode(e.Content));
                if (!e.IsUnclosed) {
                    w.Write("'");
                }

                break;
            }
            case CommentToken e: {
                if (e.IsMultiline) {
                    w.Write("/*" + e.Content);
                    if (!e.IsUnclosed) {
                        w.Write("*/");
                    }
                }
                else {
                    w.Write("//" + e.Content);
                }

                break;
            }
            case StringToken e: {
                if (e.HasPrefix("f")) {
                    w.Write("$");
                }
                w.Write("\"", HttpUtility.JavaScriptStringEncode(e.Content));
                if (!e.IsUnclosed) {
                    w.Write("\"");
                }
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
            case ClassDef e: {
                w.Write("public class ", e.Name);
                var constrainedTypeParams = new List<GenericParameterTypeName>();
                if (e.TypeParameters.Count > 0) {
                    w.Write("<");
                    for (var i = 0; i < e.TypeParameters.Count; i++) {
                        var tp = e.TypeParameters[i];
                        if (tp is GenericParameterTypeName gp) {
                            constrainedTypeParams.Add(gp);
                            w.Write(gp.Name);
                        }
                        else {
                            w.Write(tp);
                        }
                        if (i != e.TypeParameters.Count - 1) {
                            w.Write(", ");
                        }
                    }
                    w.Write(">");
                }
                if (e.Bases.Count > 0) {
                    w.Write(" : ");
                    w.AddJoin(", ", e.Bases);
                }
                if (constrainedTypeParams.Count > 0) {
                    var multiline = constrainedTypeParams.Count > 1;
                    if (multiline) {
                        w.IndentLevel++;
                        w.WriteLine();
                    }
                    else {
                        w.Write(" ");
                    }
                    foreach (var ctp in constrainedTypeParams) {
                        w.Write("where ", ctp.Name, " : ");
                        w.AddJoin(", ", ctp.TypeConstraints);
                        w.WriteLine();
                    }
                    if (multiline) {
                        w.IndentLevel--;
                    }
                }
                w.MaybeWriteLine();
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
                        e.Name
                    );
                    var constrainedTypeParams = new List<GenericParameterTypeName>();
                    if (e.TypeParameters.Count > 0) {
                        w.Write("<");
                        for (var i = 0; i < e.TypeParameters.Count; i++) {
                            var tp = e.TypeParameters[i];
                            if (tp is GenericParameterTypeName gp) {
                                constrainedTypeParams.Add(gp);
                                w.Write(gp.Name);
                            }
                            else {
                                w.Write(tp);
                            }
                            if (i != e.TypeParameters.Count - 1) {
                                w.Write(", ");
                            }
                        }
                        w.Write(">");
                    }
                    w.Write("(");
                    w.AddJoin(", ", e.Parameters);
                    w.Write(")");
                    if (constrainedTypeParams.Count > 0) {
                        var multiline = constrainedTypeParams.Count > 1;
                        if (multiline) {
                            w.IndentLevel++;
                            w.WriteLine();
                        }
                        else {
                            w.Write(" ");
                        }
                        foreach (var ctp in constrainedTypeParams) {
                            w.Write("where ", ctp.Name, " : ");
                            w.AddJoin(", ", ctp.TypeConstraints);
                            w.WriteLine();
                        }
                        if (multiline) {
                            w.IndentLevel--;
                        }
                    }
                    w.MaybeWriteLine();
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
                if (e.Parent is not FunctionDef { Name: null }) {
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
                if (e.Operator!.Is(DoubleStar)) {
                    w.Write(
                        "Math.Pow(",
                        e.Left,
                        ", ",
                        e.Right,
                        ")"
                    );
                }
                else if (e.Operator.Is(In)) {
                    // in (list1 or|and list2)
                    if (e.Right is TupleExpr paren
                     && paren.Expressions.Count == 1
                     && paren.Expressions[0] is BinaryExpr collections
                     && collections.Operator!.Is(And, Or)) {
                        w.Write(
                            collections.Right,
                            ".Contains(",
                            e.Left,
                            ") ",
                            TargetSpecification.BinaryOperators
                                [collections.Operator.Type],
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
                    if (!TargetSpecification.BinaryOperators.TryGetValue(
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
            case TupleTypeName e: {
                if (e.Types.Count == 0) {
                    w.Write("void");
                }
                else {
                    w.Write("(");
                    w.AddJoin(", ", e.Types);
                    w.Write(")");
                }
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

                var rootItems     = new NodeList<Node>(e);
                var rootClasses   = new List<Node>();
                var rootFunctions = new List<Node>();
                foreach (var expr in e.Items) {
                    var actualType = expr;
                    if (expr is DecoratedExpr dec) {
                        actualType = dec.Target!;
                    }

                    switch (actualType) {
                    case ModuleDef:
                        w.Write(expr);
                        break;
                    case ClassDef:
                        rootClasses.Add(expr);
                        break;
                    case FunctionDef:
                        rootFunctions.Add(expr);
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
                                }.Union<Node>(rootFunctions)
                            )
                        }.Union(rootClasses)
                    )
                );
                break;
            }
            case DecoratedExpr e: {
                foreach (var decorator in e.Decorators) {
                    if (decorator is NameExpr n
                     && TargetSpecification.AllowedModifiers.Contains(n.ToString())) {
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
                       || item is MacroMatchExpr)
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
            case ImportExpr e: {
                foreach (var entry in e.Entries) {
                    var children = entry.Children;
                    foreach (var child in children) {
                        TranslateImportEntry(w, "", child);
                    }
                }
                break;
            }
            default: {
                return false;
            }
            }

            return true;
        }

        private static void TranslateImportEntry(
            CodeWriter       w,
            string           acc,
            ImportExpr.Entry entry
        ) {
            if (entry.Children.Count > 0) {
                foreach (var subEntry in entry.Children) {
                    TranslateImportEntry(w, acc, subEntry);
                }
            }
            else {
                w.Write("using ");
                if (entry.Alias != null) {
                    w.Write(entry.Alias, " = ");
                }
                if (string.IsNullOrEmpty(acc)) {
                    w.Write(entry.Name);
                }
                else {
                    w.Write(
                        acc,
                        ".",
                        entry.Name,
                        ";"
                    );
                }
            }
        }
    }
}
