using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class AssertStatement : Statement {
        [JsonProperty]
        internal Expression Condition { get; }

        [JsonProperty]
        internal Expression FalseExpression { get; }

        internal AssertStatement(Expression condition, Expression falseExpression, SpannedRegion start) {
            Condition       = condition;
            FalseExpression = falseExpression;

            MarkStart(start);
            MarkEnd(falseExpression ?? condition);
        }
    }
}