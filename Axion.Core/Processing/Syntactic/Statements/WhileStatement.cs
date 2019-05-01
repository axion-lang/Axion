using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         while_stmt
    ///             : ('while' test block)
    ///             | ('do' block 'while' preglobal_expr)
    ///             ['nobreak' block]
    ///     </c>
    /// </summary>
    public class WhileStatement : LoopStatement {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal WhileStatement(SyntaxTreeNode parent, bool post = false) : base(parent) {
            if (post) {
                EatStartMark(KeywordDo);
            }
            else {
                EatStartMark(KeywordWhile);
                Condition = Expression.ParsePreGlobalExpr(this);
            }

            Block = new BlockStatement(this, BlockType.Loop);
            if (post) {
                Eat(KeywordWhile);
                Condition = Expression.ParsePreGlobalExpr(this);
            }

            if (MaybeEat(KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
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