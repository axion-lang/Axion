using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         while_stmt:
    ///             'while' test block ['else' block]
    ///     </c>
    /// </summary>
    public class WhileStatement : LoopStatement {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="WhileStatement"/> from tokens.
        /// </summary>
        internal WhileStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordWhile);

            Condition = Expression.ParseTestExpr(this);
            Block     = new BlockStatement(this, BlockType.Loop);
            if (MaybeEat(TokenType.KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="WhileStatement"/> without position in source.
        /// </summary>
        internal WhileStatement(
            Expression     condition,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : base(block, noBreakBlock) {
            Condition    = condition;
            Block        = block;
            NoBreakBlock = noBreakBlock;
        }

        #endregion

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("while ", Condition, " ", Block);
            if (NoBreakBlock != null) {
                c.Write(" nobreak ", NoBreakBlock);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("while (", Condition, ") ", Block);
            if (NoBreakBlock != null) {
                Unit.ReportError("C# doesn't support 'nobreak' block", NoBreakBlock);
            }
        }

        #endregion
    }
}