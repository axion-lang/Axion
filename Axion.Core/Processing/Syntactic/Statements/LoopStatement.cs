using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements {
    public class LoopStatement : Statement {
        private BlockStatement block;

        [NotNull]
        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private BlockStatement noBreakBlock;

        public BlockStatement NoBreakBlock {
            get => noBreakBlock;
            set => SetNode(ref noBreakBlock, value);
        }

        public LoopStatement(
            [NotNull] BlockStatement block,
            BlockStatement           noBreakBlock
        ) {
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkEnd(NoBreakBlock ?? Block);
        }

        public LoopStatement(
            Token          startToken,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : this(block, noBreakBlock) {
            MarkPosition(startToken, NoBreakBlock ?? Block);
        }

        protected LoopStatement() { }

        /// <summary>
        ///     <c>
        ///         for_stmt ::=
        ///             'for'
        ///                 (expr_list 'in' test_list) |
        ///                 ([expr_stmt] ';' [expr_stmt] ';' [expr_stmt])
        ///             block ['else' block]
        ///     </c>
        /// </summary>
        internal static LoopStatement ParseFor(SyntaxTreeNode parent) {
            int startIdx = parent.Ast.Index;
            parent.Eat(TokenType.KeywordFor);

            var isForIndex = false;
            if (parent.MaybeEat(TokenType.Semicolon)) {
                // 'for' ';'
                isForIndex = true;
            }
            else {
                Expression.ParseExpression(parent);
                if (parent.MaybeEat(TokenType.Semicolon)) {
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