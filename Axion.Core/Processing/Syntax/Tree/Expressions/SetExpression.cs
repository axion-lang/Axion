using System.Collections.Generic;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class SetExpression : Expression {
        internal Expression[] Expressions;

        internal SetExpression(List<Expression> expressions, Position start, Position end)
            : base(start, end) {
            Expressions = expressions.ToArray();
        }
    }
}