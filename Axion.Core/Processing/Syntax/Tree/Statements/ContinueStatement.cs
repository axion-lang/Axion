using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ContinueStatement : Statement {
        private Expression loopName;

        [JsonProperty]
        internal Expression LoopName {
            get => loopName;
            set {
                value.Parent = this;
                loopName     = value;
            }
        }

        internal ContinueStatement(SpannedRegion kwContinue, Expression loopName = null) {
            LoopName = loopName;

            MarkStart(kwContinue);
            MarkEnd(loopName ?? kwContinue);
        }
    }
}