using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ContinueStatement : Statement {
        [JsonProperty]
        internal Expression LoopName { get; }

        internal ContinueStatement(SpannedRegion kwContinue, Expression loopName = null) {
            LoopName = loopName;

            MarkStart(kwContinue);
            MarkEnd(loopName ?? kwContinue);
        }
    }
}