using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.CodeGen {
    public class AxionToCSharpConverter : ConverterFromAxion {
        public AxionToCSharpConverter(CodeWriter cw) : base(cw) { }

        public override void Convert(NameExpr e) {
            if (e.IsSimple && e.Qualifiers[0].Content == "self") {
                cw.Write("this");
                return;
            }
            base.Convert(e);
        }

        public override void Convert(YieldExpr e) {
            cw.Write("yield return ", e.Value);
        }

        public override void Convert(ClassDef e) {
            cw.Write("public class ", e.Name);
            if (e.Bases.Count > 0) {
                cw.Write(" : ");
                cw.AddJoin(", ", e.Bases);
            }
            cw.WriteLine();
            cw.Write(e.Scope);
        }

        public override void Convert(FunctionDef e) {
            if (e.Name == null) {
                cw.Write("(");
                cw.AddJoin(", ", e.Parameters);
                cw.Write(") => ", e.Scope);
            }
            else {
                // BUG type inference stuck in infinite loop (get-value in MathExprParser)
                cw.Write(
                    "public ", e.ValueType, " ", e.Name,
                    "("
                );
                cw.AddJoin(", ", e.Parameters);
                cw.WriteLine(")");
                cw.Write(e.Scope);
            }
        }

        public override void Convert(ModuleDef e) {
            cw.Write("namespace ", e.Name);
            cw.WriteLine();
            cw.Write(e.Scope);
        }

        public override void Convert(NameDef e) {
            if (e.Value == null) {
                cw.Write(e.ValueType, " ", e.Name);
            }
            else {
                cw.Write(
                    (object) e.ValueType ?? "var", " ", e.Name, " = ",
                    e.Value
                );
            }
        }

        public override void Convert(VarDef e) {
            if (e.Value == null) {
                cw.Write(e.ValueType, " ", e.Name);
            }
            else {
                cw.Write(
                    (object) e.ValueType ?? "var", " ", e.Name, " = ",
                    e.Value
                );
            }
        }

        public override void Convert(BinaryExpr e) {
            if (e.Operator.Is(OpPower)) {
                cw.Write(
                    "Math.Pow(", e.Left, ", ", e.Right,
                    ")"
                );
            }
            else if (e.Operator.Is(OpIn)) {
                // in (list1 or|and list2)
                if (e.Right is ParenthesizedExpr paren
                 && paren.Value is BinaryExpr collections
                 && collections.Operator.Is(OpAnd, OpOr)) {
                    cw.Write(
                        collections.Right,
                        ".Contains(",
                        e.Left,
                        ") ",
                        Spec.CSharp.BinaryOperators[collections.Operator.Type],
                        " ",
                        collections.Left,
                        ".Contains(",
                        e.Left,
                        ")"
                    );
                }
                else {
                    cw.Write(e.Right, ".Contains(", e.Left, ")");
                }
            }
            else {
                if (!Spec.CSharp.BinaryOperators.TryGetValue(e.Operator.Type, out string op)) {
                    op = e.Operator.Value;
                }
                cw.Write(
                    e.Left, " ", op, " ",
                    e.Right
                );
            }
        }

        public override void Convert(TernaryExpr e) {
            cw.Write(e.Condition, " ? ", e.TrueExpr, " : ");
            if (e.FalseExpr == null) {
                cw.Write("default");
            }
            else {
                cw.Write(e.FalseExpr);
            }
        }

        public override void Convert(UnaryExpr e) {
            string op = e.Operator.Value;
            if (op == "not") {
                op = "!";
            }

            if (e.Operator.Side == InputSide.Right) {
                cw.Write(op, " (", e.Value, ")");
            }
            else {
                cw.Write("(", e.Value, ") ", op);
            }
        }

        public override void Convert(ForComprehension e) {
            cw.Write("from ", e.Item, " in ", e.Iterable);
            if (e.Right != null) {
                cw.Write(" ", e.Right);
            }

            if (e.Conditions.Count > 0) {
                cw.Write(" where ");
                cw.AddJoin(" && ", e.Conditions);
            }

            if (!e.IsNested) {
                cw.Write(" select ", e.Target);
            }
        }

        public override void Convert(FunctionParameter e) {
            if (!(e.Parent is FunctionDef f && f.Name == null)) {
                cw.Write(e.ValueType, " ");
            }

            cw.Write(e.Name);
            if (e.Value != null) {
                cw.Write(" = ", e.Value);
            }
        }

        public override void Convert(FuncCallArg e) {
            if (e.Name != null) {
                cw.Write(e.Name, ": ");
            }

            cw.Write(e.Value);
        }

        public override void Convert(EmptyExpr e) {
            // Don't write anything, semicolon is inserted at the scope level.
        }

        public override void Convert(WhileExpr e) {
            cw.Write("while (", e.Condition);
            cw.WriteLine(")");
            cw.Write(e.Scope);
        }

        public override void Convert(FuncTypeName e) {
            cw.Write(
                "Func<", e.ArgsType, ", ", e.ReturnType,
                ">"
            );
        }

        public override void Convert(GenericTypeName e) {
            cw.Write(e.Target, "<");
            cw.AddJoin(",", e.TypeArguments);
            cw.Write(">");
        }

        public override void Convert(Ast e) {
            if (e.Items.Count == 0) {
                return;
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
            foreach (string directive in defaultDirectives) {
                cw.WriteLine($"using {directive};");
            }

            var rootItems     = new List<Expr>();
            var rootClasses   = new List<Expr>();
            var rootFunctions = new List<Expr>();
            foreach (Expr expr in e.Items) {
                Expr item = expr;
                // decorator is just a wrapper,
                // so we need to unpack it's content.
                if (expr is DecorableExpr dec) {
                    item = dec.Target;
                }
                if (item is ModuleDef) {
                    cw.Write(expr);
                }
                else if (item is ClassDef) {
                    rootClasses.Add(expr);
                }
                else if (item is FunctionDef) {
                    rootFunctions.Add(expr);
                }
                else {
                    rootItems.Add(expr);
                }
            }

            cw.Write(
                new ModuleDef(
                    "__RootModule__",
                    ScopeExpr.FromItems(
                        new[] {
                            new ClassDef(
                                "__RootClass__",
                                scope: ScopeExpr.FromItems(
                                    new[] {
                                        new DecorableExpr(
                                            decorators: new[] { new NameExpr("static") },
                                            target: new FunctionDef(
                                                "Main",
                                                new[] {
                                                    new FunctionParameter(
                                                        "args",
                                                        new ArrayTypeName(
                                                            e,
                                                            new SimpleTypeName("string")
                                                        )
                                                    )
                                                },
                                                scope: new ScopeExpr(
                                                    e,
                                                    rootItems
                                                ),
                                                returnType: new SimpleTypeName("void")
                                            )
                                        )
                                    }.Union(rootFunctions)
                                )
                            )
                        }.Union(rootClasses)
                    )
                )
            );
        }

        public override void Convert(DecorableExpr e) {
            foreach (Expr decorator in e.Decorators) {
                if (decorator is NameExpr n
                 && Spec.CSharp.AllowedModifiers.Contains(n.ToString())) {
                    cw.Write(n, " ");
                }
            }

            cw.Write(e.Target);
        }

        public override void Convert(IfExpr e) {
            cw.Write("if (", e.Condition);
            cw.WriteLine(")");
            cw.Write(e.ThenScope);
            if (e.ElseScope != null) {
                cw.WriteLine("else");
                cw.Write(e.ElseScope);
            }
        }

        public override void Convert(ScopeExpr e) {
            cw.WriteLine("{");
            cw.IndentLevel++;
            foreach (Expr item in e.Items) {
                cw.Write(item);
                if (!(e.Parent is ClassDef || e.Parent is ModuleDef)
                 && !(item is IDefinitionExpr
                   || item is IfExpr
                   || item is WhileExpr
                   || item is MacroApplicationExpr)
                 || item is VarDef) {
                    cw.Write(";");
                }

                cw.MaybeWriteLine();
            }

            cw.IndentLevel--;
            cw.Write("}");
            cw.MaybeWriteLine();
        }
    }
}