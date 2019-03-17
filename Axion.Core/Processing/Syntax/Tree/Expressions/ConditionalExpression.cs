using Axion.Core.Specification;
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
                if (value != null) {
                    value.Parent = this;
                }

                falseExpression = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public ConditionalExpression(
            Expression condition,
            Expression trueExpression,
            Expression falseExpression
        ) {
            Condition       = condition;
            TrueExpression  = trueExpression;
            FalseExpression = falseExpression;

            MarkPosition(condition, falseExpression ?? trueExpression);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + TrueExpression + " if " + Condition + " else " + FalseExpression;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Condition + " ? " + TrueExpression + " : " + FalseExpression;
        }
    }
}