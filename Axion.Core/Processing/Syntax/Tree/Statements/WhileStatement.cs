using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WhileStatement : LoopStatement {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        internal WhileStatement(
            Token          startToken,
            Expression     condition,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : base(startToken, block, noBreakBlock) {
            Condition    = condition ?? throw new ArgumentNullException(nameof(condition));
            Block        = block ?? throw new ArgumentNullException(nameof(block));
            NoBreakBlock = noBreakBlock;

            MarkEnd(NoBreakBlock ?? Block);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + "while " + Condition + " " + Block;
            if (NoBreakBlock != null) {
                c = c + " nobreak " + NoBreakBlock;
            }

            return c;
        }
    }
}