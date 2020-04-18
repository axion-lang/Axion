using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         while-expr:
    ///             'while' infix-expr scope
    ///             ['nobreak' scope];
    ///     </c>
    /// </summary>
    public class WhileExpr : Expr, IDecorableExpr {
        private Expr condition = null!;

        public Expr Condition {
            get => condition;
            set => condition = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        private ScopeExpr? noBreakScope;

        public ScopeExpr? NoBreakScope {
            get => noBreakScope;
            set => noBreakScope = BindNullable(value);
        }

        public WhileExpr(Expr parent) : base(parent) { }

        public Expr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordWhile);
                    Condition = InfixExpr.Parse(this);
                    Scope     = new ScopeExpr(this).Parse();
                    if (Stream.MaybeEat("no-break")) {
                        NoBreakScope = new ScopeExpr(this).Parse();
                    }
                }
            );
            return this;
        }
    }
}
