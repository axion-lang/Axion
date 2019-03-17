using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class TupleExpression : MultipleExpression<Expression> {
        internal bool Expandable;

        internal TupleExpression(
            bool         expandable,
            Expression[] expressions,
            Token        start,
            Token        end
        ) {
            Expandable  = expandable;
            Expressions = expressions;
            MarkPosition(start, end);
        }

        internal TupleExpression(bool expandable, Expression[] expressions) {
            Expandable  = expandable;
            Expressions = expressions;
            if (expressions.Length > 0) {
                MarkPosition(
                    expressions[0],
                    expressions[expressions.Length - 1]
                );
            }
        }
    }
}