using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         cond_stmt:
    ///             ('if' | 'unless') test block
    ///             {'elif' test block}
    ///             ['else' block]
    ///     </c>
    /// </summary>
    public class ConditionalStatement : Statement {
        #region Properties

        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockStatement thenBlock;

        public BlockStatement ThenBlock {
            get => thenBlock;
            set => SetNode(ref thenBlock, value);
        }

        private BlockStatement? elseBlock;

        public BlockStatement? ElseBlock {
            get => elseBlock;
            set => SetNode(ref elseBlock, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ConditionalStatement"/> from tokens.
        /// </summary>
        internal ConditionalStatement(SyntaxTreeNode parent, bool elseIf = false) : base(parent) {
            var invert = false;
            if (elseIf) {
                MarkStart(Token);
            }
            else if (PeekIs(TokenType.KeywordUnless)) {
                MarkStart(TokenType.KeywordUnless);
                invert = true;
            }
            else {
                MarkStart(TokenType.KeywordIf);
            }

            Condition = Expression.ParseTestExpr(this);
            ThenBlock = new BlockStatement(this);

            if (MaybeEat(TokenType.KeywordElse)) {
                ElseBlock = new BlockStatement(this);
            }
            else if (MaybeEat(TokenType.KeywordElseIf)) {
                ElseBlock = new BlockStatement(new ConditionalStatement(this, true));
            }
            else {
                BlameInvalidSyntax(TokenType.KeywordElse, Peek);
            }

            if (invert) {
                (ThenBlock, ElseBlock) = (ElseBlock, ThenBlock);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs new <see cref="ConditionalStatement"/> without position in source.
        /// </summary>
        public ConditionalStatement(
            Expression      condition,
            BlockStatement  thenBlock,
            BlockStatement? elseBlock
        ) {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;

            MarkEnd(ElseBlock ?? ThenBlock);
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("if ", Condition, " ", ThenBlock);
            if (ElseBlock != null) {
                c.Write("else ", ElseBlock);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("if (", Condition, ") ", ThenBlock);
            if (ElseBlock != null) {
                c.Write("else ", ElseBlock);
            }
        }

        #endregion
    }
}