using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WithStatement : Statement {
        private WithStatementItem item;

        [JsonProperty]
        internal WithStatementItem Item {
            get => item;
            set {
                value.Parent = this;
                item         = value;
            }
        }

        private Statement block;

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        internal WithStatement(WithStatementItem item, Statement block, SpannedRegion start) {
            Item  = item;
            Block = block;
            MarkStart(start);
            MarkEnd(Block);
        }
    }

    public class WithStatementItem : TreeNode {
        private Expression contextManager;

        [JsonProperty]
        internal Expression ContextManager {
            get => contextManager;
            set {
                value.Parent   = this;
                contextManager = value;
            }
        }

        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        public WithStatementItem(Position start, Expression contextManager, Expression name) {
            ContextManager = contextManager;
            Name           = name;
            MarkStart(start);
        }
    }
}