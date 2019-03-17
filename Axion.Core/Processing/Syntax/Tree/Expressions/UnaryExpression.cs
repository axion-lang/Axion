using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class UnaryExpression : Expression {
        public  OperatorToken Operator { get; }
        private Expression    expression;

        [JsonProperty]
        internal Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public UnaryExpression(OperatorToken op, Expression expression) {
            Operator   = op ?? throw new ArgumentNullException(nameof(op));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));

            MarkPosition(op, expression);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            if (Operator.Properties.InputSide == InputSide.Left) {
                return c + Expression + Operator.Value;
            }

            return c + Operator.Value + Expression;
        }
    }
}