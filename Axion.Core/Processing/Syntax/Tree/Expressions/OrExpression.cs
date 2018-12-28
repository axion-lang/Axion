using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class OrExpression : Expression {
        public Expression Left { get; }

        public Expression Right { get; }

        public OrExpression(Expression left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }
    }
}