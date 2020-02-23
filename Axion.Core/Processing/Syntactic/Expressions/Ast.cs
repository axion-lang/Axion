using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Abstract Syntax Tree built from source code.
    /// </summary>
    public class Ast : BlockExpr {
        internal List<MacroDef> Macros => Source.GetAllDefinitions().Values.OfType<MacroDef>().ToList();

        internal readonly Stack<MacroApplicationExpr> MacroApplicationParts =
            new Stack<MacroApplicationExpr>();

        internal Ast(SourceUnit src) {
            Source = src;
            Parent = this;
            Path   = new NodeTreePath(this, typeof(SourceUnit).GetProperty(nameof(SourceUnit.Ast)));
            Items  = new NodeList<Expr>(this);
        }

        internal TokenPattern NewTokenPattern(string keyword) {
            Source.RegisterCustomKeyword(keyword);
            return new TokenPattern(keyword);
        }

        internal void Parse() {
            SetSpan(
                () => {
                    while (!Stream.MaybeEat(TokenType.End) && !Stream.PeekIs(TokenType.End)) {
                        Expr item = AnyExpr.Parse(this);
                        Items.Add(item);
                        if (item is IDefinitionExpr def) {
                            Source.AddDefinition(def);
                        }
                    }
                }
            );
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
                                            new[] { new NameExpr("static") },
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