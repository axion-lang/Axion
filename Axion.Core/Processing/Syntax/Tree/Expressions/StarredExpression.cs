using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class StarredExpression : Expression {
        public Expression Value { get; }

        public StarredExpression(Position start, Expression value) {
            Value = value ?? throw new ArgumentNullException(nameof(value));

            MarkStart(start);
            MarkEnd(value);
        }
    }
}