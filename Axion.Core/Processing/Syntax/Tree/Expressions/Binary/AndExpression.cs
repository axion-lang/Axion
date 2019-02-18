using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class AndExpression : LeftRightExpression {
        public AndExpression(Expression left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + " and " + Right;
        }
    }
}