using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         ternary-expr:
    ///             multiple-expr ('if' | 'unless') infix-expr ['else' multiple-expr];
    ///     </c>
    /// </summary>
    public class TernaryExpr : InfixExpr {
        private Expr condition;

        public Expr Condition {
            get => condition;
            set => condition = Bind(value);
        }

        private Expr trueExpr;

        public Expr TrueExpr {
            get => trueExpr;
            set => trueExpr = Bind(value);
        }

        private Expr falseExpr;

        public Expr FalseExpr {
            get => falseExpr;
            set => falseExpr = Bind(value);
        }

        [NoPathTraversing]
        public override TypeName ValueType => TrueExpr.ValueType;

        internal TernaryExpr(Expr parent) : base(parent) { }

        public TernaryExpr Parse() {
            SetSpan(
                () => {
                    var invert = false;
                    if (!Stream.MaybeEat(KeywordIf)) {
                        Stream.Eat(KeywordUnless);
                        invert = true;
                    }

                    TrueExpr ??= AnyExpr.Parse(this);
                    Condition = Parse(this);
                    if (Stream.MaybeEat(KeywordElse)) {
                        FalseExpr = Multiple<InfixExpr>.ParseGenerally(this);
                    }

                    if (invert) {
                        (TrueExpr, FalseExpr) = (FalseExpr, TrueExpr);
                    }
                }
            );
            return this;
        }
    }
}