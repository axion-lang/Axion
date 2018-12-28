using System.Collections.Generic;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class TupleExpression : Expression {
        internal bool         Expandable;
        internal Expression[] Expressions;

        internal TupleExpression(bool expandable, List<Expression> expressions) {
            Expandable  = expandable;
            Expressions = expressions.ToArray();
            if (expressions.Count > 0) {
                MarkPosition(expressions[0].Span.Start, expressions[expressions.Count - 1].Span.End);
            }
        }
    }
}