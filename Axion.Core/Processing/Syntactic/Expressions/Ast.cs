using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Abstract Syntax Tree built from source code.
    /// </summary>
    public class Ast : BlockExpr {
        internal readonly List<MacroDef> Macros = new List<MacroDef>();

        internal readonly Stack<MacroApplicationExpr> MacroApplicationParts =
            new Stack<MacroApplicationExpr>();

        internal Ast(SourceUnit src) {
            Source = src;
            Parent = this;
            Path   = new NodeTreePath(this, typeof(SourceUnit).GetProperty(nameof(SourceUnit.Ast)));
            Items  = new NodeList<Expr>(this);
        }

        private TokenPattern NewTokenPattern(string keyword) {
            if (keyword.All(c => Spec.IdPart.Contains(c))) {
                for (int i = Math.Max(0, Stream.TokenIdx);
                     i < Source.TokenStream.Tokens.Count;
                     i++) {
                    Token token = Source.TokenStream.Tokens[i];
                    if (token.Value == keyword
                     && !Spec.Keywords.ContainsKey(token.Value)
                     && !Spec.Operators.ContainsKey(token.Value)) {
                        token.Type = CustomKeyword;
                    }
                }
            }

            return new TokenPattern(keyword);
        }

        internal void Parse() {
            Macros.Add(
                // 'do' block ('while'|'until') infix_expr
                new MacroDef(
                    NewTokenPattern("do"),
                    new ExpressionPattern(typeof(BlockExpr)),
                    new OrPattern(NewTokenPattern("while"), NewTokenPattern("until")),
                    new ExpressionPattern(InfixExpr.Parse)
                )
            );
            Macros.Add(
                // 'raise' type_name ['(' [infix_list] ')']
                new MacroDef(
                    NewTokenPattern("raise"),
                    new ExpressionPattern(typeof(TypeName)),
                    new OptionalPattern(
                        NewTokenPattern("("),
                        new OptionalPattern(new ExpressionPattern(InfixExpr.ParseList)),
                        NewTokenPattern(")")
                    )
                )
            );
            Macros.Add(
                // 'until' infix_expr block
                new MacroDef(
                    NewTokenPattern("until"),
                    new ExpressionPattern(InfixExpr.Parse),
                    new ExpressionPattern(typeof(BlockExpr))
                )
            );
            Macros.Add(
                // 'for' atom_expr 'in' infix_expr block
                new MacroDef(
                    NewTokenPattern("for"),
                    new ExpressionPattern(AtomExpr.Parse),
                    NewTokenPattern("in"),
                    new ExpressionPattern(InfixExpr.Parse),
                    new ExpressionPattern(typeof(BlockExpr))
                )
            );
            Macros.Add(
                // 'unless' infix_expr block [{'elif' infix_expr block} 'else' block]
                new MacroDef(
                    NewTokenPattern("unless"),
                    new ExpressionPattern(InfixExpr.Parse),
                    new ExpressionPattern(typeof(BlockExpr)),
                    new OptionalPattern(
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern("elif"),
                                new ExpressionPattern(InfixExpr.Parse),
                                new ExpressionPattern(typeof(BlockExpr))
                            )
                        ),
                        new CascadePattern(
                            NewTokenPattern("else"),
                            new ExpressionPattern(typeof(BlockExpr))
                        )
                    )
                )
            );
            Macros.Add(
                // '[' [infix_list [',']] ']'
                new MacroDef(
                    NewTokenPattern("["),
                    new OptionalPattern(
                        new ExpressionPattern(InfixExpr.ParseList),
                        new OptionalPattern(NewTokenPattern(","))
                    ),
                    NewTokenPattern("]")
                )
            );
            Macros.Add(
                // '{' [infix_expr ':' infix_expr {',' infix_expr ':' infix_expr}] '}'
                new MacroDef(
                    NewTokenPattern("{"),
                    new OptionalPattern(
                        new ExpressionPattern(InfixExpr.Parse),
                        NewTokenPattern(":"),
                        new ExpressionPattern(InfixExpr.Parse),
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern(","),
                                new ExpressionPattern(InfixExpr.Parse),
                                NewTokenPattern(":"),
                                new ExpressionPattern(InfixExpr.Parse)
                            )
                        ),
                        new OptionalPattern(NewTokenPattern(","))
                    ),
                    NewTokenPattern("}")
                )
            );
            Macros.Add(
                // '{' [infix_list [',']] '}'
                new MacroDef(
                    NewTokenPattern("{"),
                    new OptionalPattern(
                        new ExpressionPattern(InfixExpr.ParseList),
                        new OptionalPattern(NewTokenPattern(","))
                    ),
                    NewTokenPattern("}")
                )
            );
            Macros.Add(
                // 'new' (('(' infix_list ')') | (type_name ['(' infix_list ')'] ['{' infix_list '}']))
                new MacroDef(
                    NewTokenPattern("new"),
                    new OrPattern(
                        new CascadePattern(
                            NewTokenPattern("("),
                            new OptionalPattern(new ExpressionPattern(InfixExpr.ParseList)),
                            NewTokenPattern(")")
                        ),
                        new CascadePattern(
                            new ExpressionPattern(typeof(TypeName)),
                            new OptionalPattern(
                                NewTokenPattern("("),
                                new OptionalPattern(new ExpressionPattern(InfixExpr.ParseList)),
                                NewTokenPattern(")")
                            ),
                            new OptionalPattern(
                                NewTokenPattern("{"),
                                new OptionalPattern(new ExpressionPattern(InfixExpr.ParseList)),
                                NewTokenPattern("}")
                            )
                        )
                    )
                )
            );
            Macros.Add(
                // expr 'match' (infix_expr ':' expr)+
                new MacroDef(
                    new ExpressionPattern(typeof(Expr)),
                    NewTokenPattern("match"),
                    new MultiplePattern(
                        new ExpressionPattern(InfixExpr.Parse),
                        NewTokenPattern(":"),
                        new ExpressionPattern(typeof(Expr))
                    )
                )
            );
            SetSpan(() => {
                while (!Stream.MaybeEat(TokenType.End) && !Stream.PeekIs(TokenType.End)) {
                    Items.Add(AnyExpr.Parse(this));
                }
            });
        }

        public override void ToAxion(CodeWriter c) {
            c.AddJoin("\n", Items);
        }

        public override void ToCSharp(CodeWriter c) {
            if (Items.Count == 0) {
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
                c.WriteLine($"using {directive};");
            }

            var rootItems     = new List<Expr>();
            var rootClasses   = new List<Expr>();
            var rootFunctions = new List<Expr>();
            foreach (Expr e in Items) {
                if (e is ModuleDef) {
                    c.Write(e);
                }
                else if (e is ClassDef) {
                    rootClasses.Add(e);
                }
                else if (e is FunctionDef) {
                    rootFunctions.Add(e);
                }
                else {
                    rootItems.Add(e);
                }
            }

            c.Write(
                new ModuleDef(
                    this,
                    new NameExpr("__RootModule__"),
                    new BlockExpr(
                        this,
                        new[] {
                            new ClassDef(
                                this,
                                new NameExpr("__RootClass__"),
                                block: new BlockExpr(
                                    this,
                                    new[] {
                                        new DecoratedExpr(
                                            this,
                                            new[] {
                                                new NameExpr("static")
                                            },
                                            new FunctionDef(
                                                this,
                                                new NameExpr("Main"),
                                                new[] {
                                                    new FunctionParameter(
                                                        this,
                                                        new NameExpr("args"),
                                                        new ArrayTypeName(
                                                            this,
                                                            new SimpleTypeName("string")
                                                        )
                                                    )
                                                },
                                                block: new BlockExpr(
                                                    this,
                                                    rootItems.ToArray()
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

        public override void ToPython(CodeWriter c) {
            c.AddJoin("\n", Items);
        }

        public override void ToPascal(CodeWriter c) {
            c.WriteLine("program PascalFromAxion;");
            c.WriteLine("var x, y: integer;");
            c.WriteLine("begin");
            c.Indent++;
            c.AddJoin("", Items, true);
            c.Indent--;
            c.WriteLine("end.");
        }
    }
}