using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         conditional-expr:
    ///             'if' infix-expr scope
    ///             {'elif' infix-expr scope}
    ///             ['else' scope];
    ///     </c>
    /// </summary>
    public class IfExpr : Expr {
        private Token? branchKw;

        public Token? BranchKw {
            get => branchKw;
            set => branchKw = BindNullable(value);
        }

        private Expr? condition;

        public Expr? Condition {
            get => condition;
            set => condition = BindNullable(value);
        }

        private ScopeExpr? thenScope;

        public ScopeExpr? ThenScope {
            get => thenScope;
            set => thenScope = BindNullable(value);
        }

        private ScopeExpr? elseScope;

        public ScopeExpr? ElseScope {
            get => elseScope;
            set => elseScope = BindNullable(value);
        }

        internal IfExpr(Node parent) : base(parent) { }

        public IfExpr Parse(bool elseIf = false) {
            if (!elseIf) {
                BranchKw = Stream.Eat(KeywordIf);
            }

            Condition = InfixExpr.Parse(this);
            ThenScope = new ScopeExpr(this).Parse();

            if (Stream.MaybeEat(KeywordElse)) {
                ElseScope = new ScopeExpr(this).Parse();
            }
            else if (Stream.MaybeEat(KeywordElif)) {
                ElseScope = new ScopeExpr(this) {
                    Items = new NodeList<Expr>(this) {
                        new IfExpr(this) {
                            BranchKw = Stream.Token
                        }.Parse(true)
                    }
                };
            }

            return this;
        }
    }
}
