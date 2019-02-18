using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class OrExpression : LeftRightExpression {
        public OrExpression(Expression left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }
    }
}