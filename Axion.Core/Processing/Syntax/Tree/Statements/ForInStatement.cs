using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForInStatement : ForStatement {
        [JsonProperty]
        public Expression Left { get; }

        private Expression list;

        [JsonProperty]
        internal Expression List {
            get => list;
            set {
                value.Parent = this;
                list = value;
            }
        }

        public ForInStatement(
            Expression left,
            Expression list,
            BlockStatement block,
            BlockStatement noBreakBlock,
            SpannedRegion start
        ) : base(block, noBreakBlock, start) {
            Left = left;
            List = list ?? throw new ArgumentNullException(nameof(list));
        }
    }
}