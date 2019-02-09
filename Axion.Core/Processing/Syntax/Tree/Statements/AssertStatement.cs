using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class AssertStatement : Statement {
        private Expression condition;

        private Expression falseExpression;

        internal AssertStatement(Expression condition, Expression falseExpression, SpannedRegion start) {
            Condition       = condition;
            FalseExpression = falseExpression;

            MarkStart(start);
            MarkEnd(falseExpression ?? condition);
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
        internal Expression FalseExpression {
            get => falseExpression;
            set {
                value.Parent    = this;
                falseExpression = value;
            }
        }
    }
}