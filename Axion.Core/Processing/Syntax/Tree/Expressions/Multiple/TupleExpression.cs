namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class TupleExpression : MultipleExpression<Expression> {
        internal bool Expandable;

        internal TupleExpression(bool expandable, Expression[] expressions) {
            Expandable  = expandable;
            Expressions = expressions;
            if (expressions.Length > 0) {
                MarkPosition(
                    expressions[0].Span.StartPosition,
                    expressions[expressions.Length - 1].Span.EndPosition
                );
            }
        }

        public Expression this[int i] => Expressions[i];
    }
}