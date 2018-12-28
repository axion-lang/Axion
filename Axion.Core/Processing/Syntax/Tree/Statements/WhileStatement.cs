using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WhileStatement : Statement {
        [JsonProperty]
        public Expression Condition { get; }

        [JsonProperty]
        public Statement Body { get; }

        [JsonProperty]
        public Statement NoBreakBody { get; }

        internal WhileStatement(Expression condition, Statement body, Statement noBreakBody, SpannedRegion start) {
            Condition   = condition;
            Body        = body;
            NoBreakBody = noBreakBody;

            MarkStart(start);
            MarkEnd(NoBreakBody ?? Body);
        }
    }
}