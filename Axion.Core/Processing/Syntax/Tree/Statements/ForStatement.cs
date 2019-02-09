using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForStatement : Statement {
        private Expression list;

        private Statement block;

        private Statement noBreakBlock;

        public ForStatement(
            Expression    left,
            Expression    list,
            Statement     block,
            Statement     noBreakBlock,
            SpannedRegion start
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

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        [JsonProperty]
        internal Statement NoBreakBlock {
            get => noBreakBlock;
            set {
                value.Parent = this;
                noBreakBlock = value;
            }
        }
    }
}