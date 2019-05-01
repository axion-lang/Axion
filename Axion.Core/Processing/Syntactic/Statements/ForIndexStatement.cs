using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         for_index_stmt:
    ///             'for' [expr] ';' [preglobal_expr] ';' [preglobal_expr]
    ///             block
    ///             ['nobreak' block]
    ///     </c>
    /// </summary>
    public class ForIndexStatement : LoopStatement {
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

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ForIndexStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordFor);

            if (!MaybeEat(Semicolon)) {
                InitStmt = Expression.ParseGlobalExpr(parent);
                Eat(Semicolon);
            }

            if (!MaybeEat(Semicolon)) {
                Condition = Expression.ParsePreGlobalExpr(this);
                Eat(Semicolon);
            }

            if (!MaybeEat(Spec.NeverExprStartTypes)) {
                IterStmt = Expression.ParsePreGlobalExpr(this);
            }

            Block = new BlockStatement(this, BlockType.Loop);
            if (MaybeEat(KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
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
    }
}