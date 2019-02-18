using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
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

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public UnaryExpression(Token op, Expression expression) {
            Operator   = op ?? throw new ArgumentNullException(nameof(op));
            Expression = expression;

            MarkPosition(op, expression);
        }
    }
}