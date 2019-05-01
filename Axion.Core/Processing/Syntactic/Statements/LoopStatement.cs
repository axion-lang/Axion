using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    public abstract class LoopStatement : Statement {
        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private BlockStatement? noBreakBlock;

        public BlockStatement? NoBreakBlock {
            get => noBreakBlock;
            set => SetNode(ref noBreakBlock, value);
        }

        public LoopStatement(
            BlockStatement  block,
            BlockStatement? noBreakBlock
        ) {
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkEnd(NoBreakBlock ?? Block);
        }

        protected LoopStatement(SyntaxTreeNode parent) : base(parent) { }

        internal static LoopStatement ParseFor(SyntaxTreeNode parent) {
            int startIdx = parent.Ast.Index;
            parent.Eat(KeywordFor);

            var isForIndex = false;
            if (parent.MaybeEat(Semicolon)) {
                // 'for' ';'
                isForIndex = true;
            }
            else {
                Expression.ParseGlobalExpr(parent);
                if (parent.MaybeEat(Semicolon)) {
                    isForIndex = true;
                }
            }

            parent.MoveTo(startIdx);

            if (isForIndex) {
                return new ForIndexStatement(parent);
            }

            return new ForInStatement(parent);
        }
    }
}