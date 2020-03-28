using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Source;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Abstract Syntax Tree built from source code.
    /// </summary>
    public class Ast : ScopeExpr {
        internal List<MacroDef> Macros => Source.GetAllDefinitions().Values.OfType<MacroDef>().ToList();

        internal readonly Stack<MacroApplicationExpr> MacroApplicationParts =
            new Stack<MacroApplicationExpr>();

        internal Ast(SourceUnit src) {
            Source = src;
            Parent = this;
            Path   = new NodeTreePath(this, typeof(SourceUnit).GetProperty(nameof(SourceUnit.Ast)));
            Items  = new NodeList<Expr>(this);
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
                Expr item = e;
                // decorator is just a wrapper,
                // so we need to unpack it's content.
                if (e is DecorableExpr dec) {
                    item = dec.Target;
                }
                if (item is ModuleDef) {
                    c.Write(e);
                }
                else if (item is ClassDef) {
                    rootClasses.Add(e);
                }
                else if (item is FunctionDef) {
                    rootFunctions.Add(e);
                }
                else {
                    rootItems.Add(e);
                }
            }

            c.Write(
                new ModuleDef(
                    "__RootModule__",
                    FromItems(
                        new[] {
                            new ClassDef(
                                "__RootClass__",
                                scope: FromItems(
                                    new[] {
                                        new DecorableExpr(
                                            decorators: new[] { new NameExpr("static") },
                                            target: new FunctionDef(
                                                "Main",
                                                new[] {
                                                    new FunctionParameter(
                                                        "args",
                                                        new ArrayTypeName(
                                                            this,
                                                            new SimpleTypeName("string")
                                                        )
                                                    )
                                                },
                                                scope: new ScopeExpr(
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
            c.IndentLevel++;
            c.AddJoin("", Items, true);
            c.IndentLevel--;
            c.WriteLine("end.");
        }
    }
}