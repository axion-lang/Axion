using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WhileStatement : Statement {
        private Expression condition;

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        private Statement body;

        [JsonProperty]
        internal Statement Body {
            get => body;
            set {
                value.Parent = this;
                body         = value;
            }
        }

        private Statement noBreakBody;

        [JsonProperty]
        internal Statement NoBreakBody {
            get => noBreakBody;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                noBreakBody = value;
            }
        }

        internal WhileStatement(Expression condition, Statement body, Statement noBreakBody, SpannedRegion start) {
            Condition   = condition;
            Body        = body;
            NoBreakBody = noBreakBody;

            MarkStart(start);
            MarkEnd(NoBreakBody ?? Body);
        }
    }
}