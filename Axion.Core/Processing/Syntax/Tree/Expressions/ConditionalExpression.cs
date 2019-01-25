using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ConditionalExpression : Expression {
        private Expression condition;

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        private Expression trueExpression;

        [JsonProperty]
        internal Expression TrueExpression {
            get => trueExpression;
            set {
                value.Parent   = this;
                trueExpression = value;
            }
        }

        private Expression falseExpression;

        [JsonProperty]
        internal Expression FalseExpression {
            get => falseExpression;
            set {
                value.Parent    = this;
                falseExpression = value;
            }
        }

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