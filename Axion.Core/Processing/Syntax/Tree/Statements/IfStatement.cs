using System;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class IfStatement : Statement {
        public IfStatement(List<IfStatementBranch> branches, Statement elseBlock) {
            if (branches.Count == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(branches));
            }

            Branches = branches.ToArray();
            Else     = elseBlock;

            MarkStart(branches[0]);
            MarkEnd(Else ?? Branches[Branches.Length - 1]);
        }

        [JsonProperty]
        public IfStatementBranch[] Branches { get; }

        [JsonProperty]
        public Statement Else { get; }
    }

    public class IfStatementBranch : Statement {
        private Expression condition;

        private Statement block;

        internal IfStatementBranch(Expression condition, Statement block, SpannedRegion start) {
            Condition = condition;
            Block     = block;

            MarkPosition(start, block);
        }

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
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
}