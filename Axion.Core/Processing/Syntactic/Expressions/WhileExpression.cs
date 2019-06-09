using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         while_stmt:
    ///             'while' infix_expr block
    ///             ['nobreak' block]
    ///     </c>
    /// </summary>
    public class WhileExpression : Expression {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockExpression block;

        public BlockExpression Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private BlockExpression noBreakBlock;

        public BlockExpression NoBreakBlock {
            get => noBreakBlock;
            set => SetNode(ref noBreakBlock, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal WhileExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(KeywordWhile);
            Condition = ParseInfixExpr(this);
            Block     = new BlockExpression(this, BlockType.Loop);
            if (MaybeEat(KeywordNoBreak)) {
                NoBreakBlock = new BlockExpression(this);
            }
            MarkEnd();
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        internal WhileExpression(
            Expression      condition,
            BlockExpression block,
            BlockExpression noBreakBlock
        ) {
            Condition    = condition;
            Block        = block;
            NoBreakBlock = noBreakBlock;
            Block        = block;
            NoBreakBlock = noBreakBlock;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("while ", Condition, " ", Block);
            if (NoBreakBlock != null) {
                c.Write(" nobreak ", NoBreakBlock);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("while (", Condition, ") ", Block);
            if (NoBreakBlock != null) {
                Unit.ReportError("C# doesn't support 'nobreak' block", NoBreakBlock);
            }
        }
    }
}