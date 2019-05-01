using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         cond_stmt:
    ///             ('if' | 'unless') preglobal_expr block
    ///             {'elif' preglobal_expr block}
    ///             ['else' block]
    ///     </c>
    /// </summary>
    public class ConditionalStatement : Statement {
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

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ConditionalStatement(SyntaxTreeNode parent, bool elseIf = false) : base(parent) {
            var invert = false;
            if (elseIf) {
                MarkStart(Token);
            }
            else if (Peek.Is(KeywordUnless)) {
                EatStartMark(KeywordUnless);
                invert = true;
            }
            else {
                EatStartMark(KeywordIf);
            }

            Condition = Expression.ParsePreGlobalExpr(this);
            ThenBlock = new BlockStatement(this);

            if (MaybeEat(KeywordElse)) {
                ElseBlock = new BlockStatement(this);
            }
            else if (MaybeEat(KeywordElseIf)) {
                ElseBlock = new BlockStatement(this, new ConditionalStatement(this, true));
            }
            else if (elseIf) {
                BlameInvalidSyntax(KeywordElse, Peek);
            }

            if (invert) {
                (ThenBlock, ElseBlock) = (ElseBlock, ThenBlock);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
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
    }
}