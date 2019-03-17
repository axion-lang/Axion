using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class ListExpression : MultipleExpression<Expression> {
        internal ListExpression(Expression[] expressions) {
            Expressions = expressions;
        }

        internal ListExpression(Expression[] expressions, Token start, Token end)
            : this(expressions) {
            MarkPosition(start, end);
        }
    }
}