using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForStatement : Statement {
        private Expression list;

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
                value.Parent = this;
                noBreakBlock = value;
            }
        }

        public ForStatement(
            Expression     left,
            Expression     list,
            BlockStatement block,
            BlockStatement noBreakBlock,
            SpannedRegion  start
        ) {
            Left         = left;
            List         = list ?? throw new ArgumentNullException(nameof(list));
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkStart(start);
            MarkEnd(NoBreakBlock ?? (SpannedRegion) Block ?? List);
        }

        [JsonProperty]
        public Expression Left { get; }

        [JsonProperty]
        internal Expression List {
            get => list;
            set {
                value.Parent = this;
                list         = value;
            }
        }
    }
}