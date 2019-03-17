using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class MapExpression : MultipleExpression<SliceExpression> {
        internal MapExpression(SliceExpression[] expressions) {
            Expressions = expressions;
        }

        internal MapExpression(SliceExpression[] expressions, Token start, Token end)
            : this(expressions) {
            MarkPosition(start, end);
        }
    }
}