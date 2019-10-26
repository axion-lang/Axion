using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
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
    ///     Abstract Syntax Tree of
    ///     file with Axion source code.
    /// </summary>
    public class Ast : BlockExpr {
        internal readonly List<MacroDef>              Macros                = new List<MacroDef>();
        internal readonly Stack<MacroApplicationExpr> MacroApplicationParts = new Stack<MacroApplicationExpr>();

        /// <summary>
        ///     Constructor for root AST block.
        /// </summary>
        internal Ast(SourceUnit src) {
            Source = src;
            Parent = this;
            Path   = new NodeTreePath(this, typeof(SourceUnit).GetProperty(nameof(SourceUnit.Ast)));
            Items  = new NodeList<Expr>(this);
        }

        internal TokenPattern NewTokenPattern(string keyword) {
            if (keyword.All(c => Spec.IdPart.Contains(c.ToString()))) {
                for (int i = Math.Max(0, Stream.TokenIdx); i < Source.TokenStream.Tokens.Count; i++) {
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
                new MacroDef(
                    NewTokenPattern("do"),
                    new ExpressionPattern(typeof(BlockExpr)),
                    new OrPattern(NewTokenPattern("while"), NewTokenPattern("until")),
                    new ExpressionPattern(Parsing.ParseInfix)
                )
            );
            Macros.Add(
                new MacroDef(
                    NewTokenPattern("until"),
                    new ExpressionPattern(Parsing.ParseInfix),
                    new ExpressionPattern(typeof(BlockExpr))
                )
            );
            Macros.Add(
                new MacroDef(
                    NewTokenPattern("for"),
                    new ExpressionPattern(Parsing.ParseAtom),
                    NewTokenPattern("in"),
                    new ExpressionPattern(Parsing.ParseInfix),
                    new ExpressionPattern(typeof(BlockExpr))
                )
            );
            Macros.Add(
                new MacroDef(
                    NewTokenPattern("unless"),
                    new ExpressionPattern(Parsing.ParseInfix),
                    new ExpressionPattern(typeof(BlockExpr)),
                    new OptionalPattern(
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern("elif"),
                                new ExpressionPattern(Parsing.ParseInfix),
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
                new MacroDef(
                    NewTokenPattern("["),
                    new OptionalPattern(
                        new ExpressionPattern(Parsing.ParseInfix),
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern(","),
                                new ExpressionPattern(Parsing.ParseInfix)
                            )
                        ),
                        new OptionalPattern(NewTokenPattern(","))
                    ),
                    NewTokenPattern("]")
                )
            );
            Macros.Add(
                new MacroDef(
                    NewTokenPattern("{"),
                    new OptionalPattern(
                        new ExpressionPattern(Parsing.ParseInfix),
                        NewTokenPattern(":"),
                        new ExpressionPattern(Parsing.ParseInfix),
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern(","),
                                new ExpressionPattern(Parsing.ParseInfix),
                                NewTokenPattern(":"),
                                new ExpressionPattern(Parsing.ParseInfix)
                            )
                        ),
                        new OptionalPattern(NewTokenPattern(","))
                    ),
                    NewTokenPattern("}")
                )
            );
            Macros.Add(
                new MacroDef(
                    NewTokenPattern("{"),
                    new OptionalPattern(
                        new ExpressionPattern(Parsing.ParseInfix),
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern(","),
                                new ExpressionPattern(Parsing.ParseInfix)
                            )
                        ),
                        new OptionalPattern(NewTokenPattern(","))
                    ),
                    NewTokenPattern("}")
                )
            );
            Macros.Add(
                new MacroDef(
                    NewTokenPattern("new"),
                    new ExpressionPattern(typeof(TypeName)),
                    new OptionalPattern(
                        NewTokenPattern("("),
                        new OptionalPattern(
                            new ExpressionPattern(Parsing.ParseInfix),
                            new OptionalPattern(
                                new MultiplePattern(
                                    NewTokenPattern(","),
                                    new ExpressionPattern(Parsing.ParseInfix)
                                )
                            )
                        ),
                        NewTokenPattern(")")
                    ),
                    new OptionalPattern(
                        NewTokenPattern("{"),
                        new OptionalPattern(
                            new ExpressionPattern(Parsing.ParseInfix),
                            new OptionalPattern(
                                new MultiplePattern(
                                    NewTokenPattern(","),
                                    new ExpressionPattern(Parsing.ParseInfix)
                                )
                            ),
                            new ExpressionPattern(Parsing.ParseInfix)
                        ),
                        NewTokenPattern("}")
                    )
                )
            );
            Macros.Add(
                new MacroDef(
                    new ExpressionPattern(typeof(Expr)),
                    NewTokenPattern("match"),
                    new MultiplePattern(
                        new ExpressionPattern(Parsing.ParseInfix),
                        NewTokenPattern(":"),
                        new ExpressionPattern(typeof(Expr))
                    )
                )
            );
            SetSpan(() => {
                while (!Stream.MaybeEat(TokenType.End) && !Stream.PeekIs(TokenType.End)) {
                    Items.Add(Parsing.ParseAny(this));
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

            if (Source.ProcessingMode == ProcessingMode.Interpret) {
                foreach (Expr e in Items) {
                    if (e is ModuleDef) {
                        LangException.Report(BlameType.ModuleNotSupportedInInterpretationMode, e);
                    }
                    else {
                        c.Write(e);
                    }

                    // Semicolon after method or accessor block is not valid
                    if (!(e is FunctionDef)) {
                        c.WriteLine(";");
                    }
                }

                return;
            }

            var rootItems   = new List<Expr>();
            var rootClasses = new List<Expr>();
            var rootFuncs   = new List<Expr>();
            foreach (Expr e in Items) {
                if (e is ModuleDef) {
                    c.Write(e);
                }
                else if (e is ClassDef) {
                    rootClasses.Add(e);
                }
                else if (e is FunctionDef) {
                    rootFuncs.Add(e);
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
                                        new FunctionDef(
                                            this,
                                            new NameExpr("Main"),
                                            block: new BlockExpr(
                                                this,
                                                rootItems.ToArray()
                                            ),
                                            returnType: new SimpleTypeName("void")
                                        )
                                    }.Union(rootFuncs).ToArray()
                                )
                            )
                        }.Union(rootClasses).ToArray()
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