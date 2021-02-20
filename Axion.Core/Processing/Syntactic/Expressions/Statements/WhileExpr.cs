using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         while-expr:
    ///             'while' infix-expr scope
    ///             ['nobreak' scope];
    ///     </c>
    /// </summary>
    public class WhileExpr : Expr, IDecorableExpr {
        private Token? kwWhile;

        public Token? KwWhile {
            get => kwWhile;
            set => kwWhile = BindNullable(value);
        }

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

        private Token? kwNoBreak;

        public Token? KwNoBreak {
            get => kwNoBreak;
            set => kwNoBreak = BindNullable(value);
        }

        private ScopeExpr? noBreakScope;

        public ScopeExpr? NoBreakScope {
            get => noBreakScope;
            set => noBreakScope = BindNullable(value);
        }

        public WhileExpr(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Expr[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Expr>(this, items)
            };
        }

        public Expr Parse() {
            KwWhile   = Stream.Eat(KeywordWhile);
            Condition = InfixExpr.Parse(this);
            Scope     = new ScopeExpr(this).Parse();
            if (Stream.MaybeEat("no-break")) {
                KwNoBreak    = Stream.Token;
                NoBreakScope = new ScopeExpr(this).Parse();
            }

            return this;
        }
    }
}
