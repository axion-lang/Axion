using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         yield-expr:
    ///             'yield' ('from' infix-expr) | multiple-infix;
    ///     </c>
    /// </summary>
    public class YieldExpr : AtomExpr {
        private Token? kwYield;

        public Token? KwYield {
            get => kwYield;
            set => kwYield = BindNullable(value);
        }

        private Token? kwFrom;

        public Token? KwFrom {
            get => kwFrom;
            set => kwFrom = BindNullable(value);
        }

        private Expr? val;

        public Expr? Value {
            get => val;
            set => val = BindNullable(value);
        }

        public YieldExpr(Node parent) : base(parent) { }

        public YieldExpr Parse() {
            KwYield ??= Stream.Eat(KeywordYield);
            if (Stream.MaybeEat("from")) {
                KwFrom = Stream.Token;
                Value  = InfixExpr.Parse(this);
            }
            else {
                Value = Multiple<InfixExpr>.ParseGenerally(this);
            }

            return this;
        }
    }
}
