using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class IfStatementBranch : Statement {
        [JsonProperty]
        internal Expression Condition { get; }

        [JsonProperty] internal Statement Body;

        internal IfStatementBranch(Expression condition, Statement body, SpannedRegion start) {
            Condition = condition;
            Body      = body;

            MarkPosition(start, body);
        }
    }
}