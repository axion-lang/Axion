using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class BreakStatement : Statement {
        private Expression loopName;

        internal BreakStatement(SpannedRegion kwBreak, Expression loopName = null) {
            LoopName = loopName;

            MarkStart(kwBreak);
            MarkEnd(loopName ?? kwBreak);
        }

        [JsonProperty]
        internal Expression LoopName {
            get => loopName;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                loopName = value;
            }
        }
    }
}