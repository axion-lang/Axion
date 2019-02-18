using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WhileStatement : Statement {
        private Expression condition;

        private BlockStatement block;

        [JsonProperty]
        internal BlockStatement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        private BlockStatement noBreakBlock;

        [JsonProperty]
        internal BlockStatement NoBreakBlock {
            get => noBreakBlock;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                noBreakBlock = value;
            }
        }

        internal WhileStatement(
            Expression     condition,
            BlockStatement block,
            BlockStatement noBreakBlock,
            SpannedRegion  start
        ) {
            Condition    = condition;
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkStart(start);
            MarkEnd(NoBreakBlock ?? Block);
        }

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }
    }
}