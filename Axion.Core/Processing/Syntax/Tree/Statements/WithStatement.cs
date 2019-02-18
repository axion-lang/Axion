using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WithStatement : Statement {
        private WithStatementItem item;

        private Statement block;

        internal WithStatement(WithStatementItem item, Statement block, SpannedRegion start) {
            Item  = item;
            Block = block;
            MarkStart(start);
            MarkEnd(Block);
        }

        [JsonProperty]
        internal WithStatementItem Item {
            get => item;
            set {
                value.Parent = this;
                item         = value;
            }
        }

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }
    }

    public class WithStatementItem : SyntaxTreeNode {
        private Expression contextManager;

        private Expression name;

        public WithStatementItem(Position start, Expression contextManager, Expression name) {
            ContextManager = contextManager;
            Name           = name;
            MarkStart(start);
        }

        [JsonProperty]
        internal Expression ContextManager {
            get => contextManager;
            set {
                value.Parent   = this;
                contextManager = value;
            }
        }

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }
    }
}