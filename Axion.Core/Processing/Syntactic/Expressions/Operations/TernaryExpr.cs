using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         ternary-expr:
    ///             multiple-expr ('if' | 'unless') infix-expr ['else' multiple-expr];
    ///     </c>
    /// </summary>
    public class TernaryExpr : InfixExpr {
        private Node? condition;

        public Node? Condition {
            get => condition;
            set => condition = BindNullable(value);
        }

        private Token? trueMark;

        public Token? TrueMark {
            get => trueMark;
            set => trueMark = BindNullable(value);
        }

        private Node? trueExpr;

        public Node? TrueExpr {
            get => trueExpr;
            set => trueExpr = BindNullable(value);
        }

        private Token? falseMark;

        public Token? FalseMark {
            get => falseMark;
            set => falseMark = BindNullable(value);
        }

        private Node? falseExpr;

        public Node? FalseExpr {
            get => falseExpr;
            set => falseExpr = BindNullable(value);
        }

        public override TypeName? ValueType =>
            TrueExpr?.ValueType ?? FalseExpr?.ValueType;

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
                FalseExpr = AnyExpr.Parse(this);
            }

            if (invert) {
                (TrueExpr, FalseExpr) = (FalseExpr!, TrueExpr);
            }

            return this;
        }
    }
}
