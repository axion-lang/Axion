using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class SetExpression : MultipleExpression<Expression> {
        internal SetExpression(Expression[] expressions) {
            Expressions = expressions;
        }

        internal SetExpression(Expression[] expressions, Token start, Token end)
            : this(expressions) {
            MarkPosition(start, end);
        }
    }
}