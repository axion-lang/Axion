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

        private Statement body;

        [JsonProperty]
        internal Statement Body {
            get => body;
            set {
                value.Parent = this;
                body         = value;
            }
        }

        internal WithStatement(WithStatementItem item, Statement body, SpannedRegion start) {
            Item = item;
            Body = body;
            MarkStart(start);
            MarkEnd(Body);
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