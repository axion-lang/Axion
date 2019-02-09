using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ConditionalExpression : Expression {
        private Expression condition;

        private Expression trueExpression;

        private Expression falseExpression;

        public ConditionalExpression(Expression condition, Expression trueExpression, Expression falseExpression) {
            Condition       = condition;
            TrueExpression  = trueExpression;
            FalseExpression = falseExpression;

            MarkPosition(condition, falseExpression ?? trueExpression);
        }

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        [JsonProperty]
        internal Expression TrueExpression {
            get => trueExpression;
            set {
                value.Parent   = this;
                trueExpression = value;
            }
        }

        [JsonProperty]
        internal Expression FalseExpression {
            get => falseExpression;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                falseExpression = value;
            }
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return TrueExpression + " if " + Condition + " else " + FalseExpression;
        }
    }
}