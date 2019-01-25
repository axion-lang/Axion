using System;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class UnaryExpression : Expression {
        public Token Operator { get; }

        private Expression expression;

        [JsonProperty]
        internal Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        public UnaryExpression(Token op, Expression expression) {
            Operator   = op ?? throw new ArgumentNullException(nameof(op));
            Expression = expression;

            MarkPosition(op, expression);
        }
    }
}