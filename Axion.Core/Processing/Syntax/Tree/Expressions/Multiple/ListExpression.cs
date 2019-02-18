namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class ListExpression : MultipleExpression<Expression> {
        internal ListExpression(Span region, Expression[] expressions) {
            Expressions = expressions;
            Span        = region;
        }
    }
}