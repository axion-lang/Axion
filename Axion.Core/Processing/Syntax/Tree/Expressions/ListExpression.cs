using System.Collections.Generic;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ListExpression : Expression {
        internal List<Expression> Expressions;

        internal ListExpression(Span region, List<Expression> expressions) {
            Expressions = expressions;
            Span        = region;
        }
    }
}