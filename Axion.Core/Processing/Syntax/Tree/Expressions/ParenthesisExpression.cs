namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ParenthesisExpression : Expression {
        internal Expression Expression { get; }

        internal override string CannotDeleteReason => Expression.CannotDeleteReason;

        internal ParenthesisExpression(Expression expression) {
            Expression = expression;
            MarkPosition(expression);
        }
    }
}