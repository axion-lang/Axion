using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         ternary-expr:
    ///             multiple-expr ('if' | 'unless') infix-expr ['else' multiple-expr];
    ///     </c>
    /// </summary>
    public class TernaryExpr : InfixExpr {
        private Expr condition = null!;

        public Expr Condition {
            get => condition;
            set => condition = Bind(value);
        }

        private Token? trueMark;

        public Token? TrueMark {
            get => trueMark;
            set => trueMark = BindNullable(value);
        }

        private Expr trueExpr = null!;

        public Expr TrueExpr {
            get => trueExpr;
            set => trueExpr = Bind(value);
        }

        private Token? falseMark;

        public Token? FalseMark {
            get => falseMark;
            set => falseMark = BindNullable(value);
        }

        private Expr falseExpr = null!;

        public Expr FalseExpr {
            get => falseExpr;
            set => falseExpr = Bind(value);
        }

        public override TypeName ValueType => TrueExpr.ValueType;

        internal TernaryExpr(Node parent) : base(parent) { }

        public TernaryExpr Parse() {
            var invert = false;
            if (!Stream.MaybeEat(KeywordIf)) {
                Stream.Eat(KeywordUnless);
                invert = true;
            }
            TrueMark  =   Stream.Token;
            TrueExpr  ??= AnyExpr.Parse(this);
            Condition =   Parse(this);
            if (Stream.MaybeEat(KeywordElse)) {
                FalseMark = Stream.Token;
                FalseExpr = Multiple<InfixExpr>.ParseGenerally(this);
            }
            if (invert) {
                (TrueExpr, FalseExpr) = (FalseExpr, TrueExpr);
            }
            return this;
        }
    }
}
