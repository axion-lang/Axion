using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ConditionalExpression : Expression {
        [JsonProperty]
        internal Expression Condition { get; }

        [JsonProperty]
        internal Expression TrueExpression { get; }

        [JsonProperty]
        internal Expression FalseExpression { get; }

        public ConditionalExpression(Expression condition, Expression trueExpression, Expression falseExpression) {
            Condition       = condition;
            TrueExpression  = trueExpression;
            FalseExpression = falseExpression;

            MarkPosition(condition, falseExpression ?? trueExpression);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return TrueExpression + " if " + Condition + " else " + FalseExpression;
        }
    }
}