namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class SetExpression : MultipleExpression<Expression> {
        internal SetExpression(Expression[] expressions, Position start, Position end) : base(
            start,
            end
        ) {
            Expressions = expressions;
        }
    }
}