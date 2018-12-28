using System;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class UnaryExpression : Expression {
        public Token Op { get; }

        public Expression Expression { get; }

        public UnaryExpression(Token op, Expression expression) {
            Op         = op ?? throw new ArgumentNullException(nameof(op));
            Expression = expression;

            MarkPosition(op, expression);
        }
    }
}