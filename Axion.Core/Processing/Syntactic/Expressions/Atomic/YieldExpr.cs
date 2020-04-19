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
        private Expr? val;

        public Expr? Value {
            get => val;
            set => val = BindNullable(value);
        }

        public bool IsYieldFrom { get; set; }

        public YieldExpr(Node parent) : base(parent) { }

        public YieldExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordYield);
                    IsYieldFrom = Stream.MaybeEat("from");
                    Value = IsYieldFrom
                        ? InfixExpr.Parse(this)
                        : Multiple<InfixExpr>.ParseGenerally(this);
                }
            );
            return this;
        }
    }
}
