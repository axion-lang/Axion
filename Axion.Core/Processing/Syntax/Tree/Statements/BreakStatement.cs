using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class BreakStatement : Statement {
        [JsonProperty]
        internal Expression LoopName { get; }

        internal BreakStatement(SpannedRegion kwBreak, Expression loopName = null) {
            LoopName = loopName;

            MarkStart(kwBreak);
            MarkEnd(loopName ?? kwBreak);
        }
    }
}