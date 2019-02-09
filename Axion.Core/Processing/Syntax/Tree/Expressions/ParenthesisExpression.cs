using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ParenthesisExpression : Expression {
        private Expression expression;

        internal ParenthesisExpression(Expression expression) {
            Expression = expression;
            MarkPosition(expression);
        }

        [JsonProperty]
        internal Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        internal override string CannotDeleteReason => Expression.CannotDeleteReason;

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return "(" + Expression + ")";
        }
    }
}