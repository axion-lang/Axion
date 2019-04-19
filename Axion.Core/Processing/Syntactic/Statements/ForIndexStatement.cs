using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         for_index_stmt:
    ///             'for' [expr] ';' [expr] ';' [expr] block
    ///     </c>
    /// </summary>
    public class ForIndexStatement : LoopStatement {
        #region Properties

        private Expression? initStmt;

        public Expression? InitStmt {
            get => initStmt;
            set => SetNode(ref initStmt, value);
        }

        private Expression? condition;

        public Expression? Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private Expression? iterStmt;

        public Expression? IterStmt {
            get => iterStmt;
            set => SetNode(ref iterStmt, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ForIndexStatement"/> from tokens.
        /// </summary>
        internal ForIndexStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordFor);

            if (!MaybeEat(TokenType.Semicolon)) {
                InitStmt = Expression.ParseExpression(parent);
                Eat(TokenType.Semicolon);
            }

            if (!MaybeEat(TokenType.Semicolon)) {
                Condition = Expression.ParseTestExpr(this);
                Eat(TokenType.Semicolon);
            }

            if (!MaybeEat(Spec.NeverTestTypes)) {
                IterStmt = Expression.ParseExpression(this);
            }

            Block = new BlockStatement(this, BlockType.Loop);
            if (MaybeEat(TokenType.KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="ForIndexStatement"/> without position in source.
        /// </summary>
        public ForIndexStatement(
            BlockStatement  block,
            Expression?     initStmt     = null,
            Expression?     condition    = null,
            Expression?     iterStmt     = null,
            BlockStatement? noBreakBlock = null
        ) : base(block, noBreakBlock) {
            InitStmt  = initStmt;
            Condition = condition;
            IterStmt  = iterStmt;
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(
                "for ",
                InitStmt,
                "; ",
                Condition,
                "; ",
                IterStmt,
                " ",
                Block
            );
            if (NoBreakBlock != null) {
                c.Write(" nobreak ", NoBreakBlock);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(
                "for (",
                InitStmt,
                "; ",
                Condition,
                "; ",
                IterStmt,
                ") ",
                Block
            );
            if (NoBreakBlock != null) {
                Unit.ReportError("C# doesn't support 'nobreak' block", NoBreakBlock);
            }
        }

        #endregion
    }
}