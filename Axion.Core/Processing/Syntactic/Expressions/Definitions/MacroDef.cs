using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Patterns;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         macro-def:
    ///             'macro' simple-name syntax-description scope;
    ///     </c>
    /// </summary>
    public class MacroDef : Node, IDefinitionExpr {
        private Token? kwMacro;

        public Token? KwMacro {
            get => kwMacro;
            set => kwMacro = BindNullable(value);
        }

        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private CascadePattern syntax = null!;

        public CascadePattern Syntax {
            get => syntax;
            set => syntax = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        public Dictionary<string, string> NamedSyntaxParts { get; } = new();

        internal MacroDef(Node parent) : base(parent) { }

        public MacroDef Parse() {
            // TODO: find code, that can be replaced with macro by patterns
            // Example:
            // ========
            // macro post-condition-loop (
            //   'do',
            //   scope: Scope,
            //   ('while' | 'until'),
            //   condition: Infix
            // )
            //   if syntax[2] == 'while'
            //     condition = {{ not $condition }}
            // 
            //   return {{
            //     while true {
            //       $scope
            //       if $condition {
            //         break
            //       }
            //     }
            //   }}
            KwMacro = Stream.Eat(KeywordMacro);
            Name    = new NameExpr(this).Parse(true);
            Syntax  = new CascadePattern(this);
            // EBNF-based syntax definition
            if (Stream.MaybeEat(OpenParenthesis)) {
                Syntax.Parse();
                Stream.Eat(CloseParenthesis);
            }

            Scope = new ScopeExpr(this).Parse();
            return this;
        }
    }
}
