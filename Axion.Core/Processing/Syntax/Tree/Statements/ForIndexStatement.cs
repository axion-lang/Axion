using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForIndexStatement : ForStatement {
        private Expression initStmt;

        [JsonProperty]
        internal Expression InitStmt {
            get => initStmt;
            set {
                value.Parent = this;
                initStmt     = value;
            }
        }

        private Expression condition;

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        private Expression iterStmt;

        [JsonProperty]
        internal Expression IterStmt {
            get => iterStmt;
            set {
                value.Parent = this;
                iterStmt     = value;
            }
        }

        public ForIndexStatement(
            Expression     initStmt,
            Expression     condition,
            Expression     iterStmt,
            BlockStatement block,
            BlockStatement noBreakBlock,
            SpannedRegion  start
        ) : base(block, noBreakBlock, start) {
            InitStmt  = initStmt;
            Condition = condition;
            IterStmt  = iterStmt;
        }
    }
}