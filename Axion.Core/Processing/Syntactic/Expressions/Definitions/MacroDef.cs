using System;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         macro_def:
    ///         'macro' simple_name block;
    ///     </c>
    ///     TODO fix macro def syntax
    /// </summary>
    public class MacroDef : Expr, IDefinitionExpr {
        private BlockExpr block;
        private NameExpr  name;

        private NodeList<FunctionParameter> parameters;
        internal MacroDef(Expr parent) : base(parent) { }

        internal MacroDef(params IPattern[] patterns) {
            Syntax = new CascadePattern(patterns);
        }

        public NodeList<FunctionParameter> Parameters {
            get => parameters;
            set => SetNode(ref parameters, value);
        }

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public CascadePattern Syntax { get; private set; }

        public NameExpr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        public MacroDef Parse() {
            SetSpan(
                () => {
                    // TODO: find code, that can be replaced with macro by patterns
                    // Example:
                    // ========
                    // macro post-condition-loop (
                    //     block:     Syntax.BlockExpr,
                    //     condition: Syntax.InfixExpr,
                    // )
                    //     syntax = $('do', block, ('while' | 'until'), condition)
                    //
                    //     if syntax[2] == 'while'
                    //         condition = {{not $condition}}
                    //
                    //     return {{
                    //         while true {
                    //             $block
                    //             if $condition {
                    //                 break
                    //             }
                    //         }
                    //     }}
                    Stream.Eat(KeywordMacro);
                    Name = new NameExpr(this).Parse(true);
                    // parameters
                    if (Stream.MaybeEat(OpenParenthesis)) {
                        Parameters = FunctionDef.ParseParameterList(
                            this,
                            CloseParenthesis
                        );
                        Stream.Eat(CloseParenthesis);
                    }
                    else {
                        Parameters = new NodeList<FunctionParameter>(this);
                    }
                    Block                    = new BlockExpr(this).Parse();
                    (VarDef syntaxDef, _, _) = Block.FindItemsOfType<VarDef>().FirstOrDefault();
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
            throw new NotSupportedException();
        }

        public override void ToCSharp(CodeWriter c) {
            throw new NotSupportedException();
        }
    }
}