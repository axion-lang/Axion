using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class ContinueStatement : Statement {
        private Expression loopName;

        internal ContinueStatement(SpannedRegion kwContinue, Expression loopName = null) {
            LoopName = loopName;

            MarkStart(kwContinue);
            MarkEnd(loopName ?? kwContinue);
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