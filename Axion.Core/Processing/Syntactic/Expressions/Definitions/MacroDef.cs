using System;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         macro-def:
    ///         'macro' simple-name scope;
    ///     </c>
    ///     TODO fix macro def syntax
    /// </summary>
    public class MacroDef : Expr, IDefinitionExpr {
        public CascadePattern Syntax { get; private set; }

        private NodeList<FunctionParameter> parameters;

        public NodeList<FunctionParameter> Parameters {
            get => parameters;
            set => SetNode(ref parameters, value);
        }

        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => SetNode(ref scope, value);
        }

        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        internal MacroDef(Expr parent) : base(parent) {
            Syntax     = new CascadePattern();
            Parameters = NodeList<FunctionParameter>.From(this, parameters);
        }

        public MacroDef Parse() {
            SetSpan(
                () => {
                    // TODO: find code, that can be replaced with macro by patterns
                    // Example:
                    // ========
                    // macro post-condition-loop (
                    //     scope:     Syntax.Scope,
                    //     condition: Syntax.Infix,
                    // )
                    //     syntax = $('do', scope, ('while' | 'until'), condition)
                    //
                    //     if syntax[2] == 'while'
                    //         condition = {{ not $condition }}
                    //
                    //     return {{
                    //         while true {
                    //             $scope
                    //             if $condition {
                    //                 break
                    //             }
                    //         }
                    //     }}
                    Stream.Eat(KeywordMacro);
                    Name = new NameExpr(this).Parse(true);
                    // parameters
                    if (Stream.MaybeEat(OpenParenthesis)) {
                        Parameters = FunctionParameter.ParseList(
                            this,
                            CloseParenthesis
                        );
                        Stream.Eat(CloseParenthesis);
                    }
                    else {
                        Parameters = new NodeList<FunctionParameter>(this);
                    }
                    Scope                    = new ScopeExpr(this).Parse();
                    (VarDef syntaxDef, _, _) = Scope.FindItemsOfType<VarDef>().FirstOrDefault();
                    if (syntaxDef                 != null
                     && syntaxDef.Name.ToString() == "syntax"
                     && syntaxDef.Value is EBNFSyntaxExpr syntaxExpr) {
                        Syntax = syntaxExpr.Syntax;
                    }
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("macro ", Name);
            if (Parameters.Count > 0) {
                c.Write("(");
                c.AddJoin(", ", Parameters);
                c.Write(") ");
            }
            c.Write(Scope);
        }

        public override void ToCSharp(CodeWriter c) {
            throw new NotSupportedException();
        }
    }
}