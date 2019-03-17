using System;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class LoopStatement : Statement {
        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        private BlockStatement noBreakBlock;

        public BlockStatement NoBreakBlock {
            get => noBreakBlock;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                noBreakBlock = value;
            }
        }

        public LoopStatement(
            Token          startToken,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : base(startToken) {
            Block        = block ?? throw new ArgumentNullException(nameof(block));
            NoBreakBlock = noBreakBlock;

            MarkEnd(NoBreakBlock ?? Block);
        }
    }
}