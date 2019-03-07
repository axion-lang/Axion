using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForStatement : Statement {
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

        public ForStatement(
            BlockStatement block,
            BlockStatement noBreakBlock,
            SpannedRegion  start
        ) {
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkStart(start);
            MarkEnd(NoBreakBlock ?? Block);
        }

        protected ForStatement() { }
    }
}