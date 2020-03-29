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
        private Expr condition;

        public Expr Condition {
            get => condition;
            set => condition = Bind(value);
        }

        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        private ScopeExpr? noBreakScope;

        public ScopeExpr? NoBreakScope {
            get => noBreakScope;
            set => noBreakScope = Bind(value);
        }

        public WhileExpr(
            Expr?      parent       = null,
            Expr?      condition    = null,
            ScopeExpr? scope        = null,
            ScopeExpr? noBreakScope = null
        ) : base(
            parent
         ?? GetParentFromChildren(
                condition,
                scope,
                noBreakScope
            )
        ) {
            Condition    = condition;
            Scope        = scope;
            NoBreakScope = noBreakScope;
        }

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