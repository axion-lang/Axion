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

        private Statement block;

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                value.Parent = this;
                block         = value;
            }
        }

        private Statement noBreakBlock;

        [JsonProperty]
        internal Statement NoBreakBlock {
            get => noBreakBlock;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                noBreakBlock = value;
            }
        }

        internal WhileStatement(Expression condition, Statement block, Statement noBreakBlock, SpannedRegion start) {
            Condition   = condition;
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkStart(start);
            MarkEnd(NoBreakBlock ?? Block);
        }
    }
}