using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         tuple_expr ::=
    ///             ['('] expr* [')']
    ///     </c>
    /// </summary>
    public class TupleExpression : MultipleExpression<Expression> {
        internal bool Expandable;

        internal TupleExpression(
            bool                 expandable,
            NodeList<Expression> expressions,
            Token                start,
            Token                end
        ) {
            Expandable  = expandable;
            Expressions = expressions ?? new TestList(this, out bool _).Expressions;
            MarkPosition(start, end);
        }

        internal TupleExpression(bool expandable, NodeList<Expression> expressions) {
            Expandable  = expandable;
            Expressions = expressions ?? new TestList(this, out bool _).Expressions;
            if (Expressions.Count > 0) {
                MarkPosition(
                    Expressions[0],
                    Expressions[Expressions.Count - 1]
                );
            }
        }
    }
}