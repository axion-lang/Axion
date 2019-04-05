using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         while_stmt ::=
    ///             'while' test block ['else' block]
    ///     </c>
    /// </summary>
    public class WhileStatement : LoopStatement {
        private Expression condition;

        [NotNull]
        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        internal WhileStatement(
            [NotNull] Expression     condition,
            [NotNull] BlockStatement block,
            BlockStatement           noBreakBlock
        ) : base(block, noBreakBlock) {
            Condition    = condition;
            Block        = block;
            NoBreakBlock = noBreakBlock;

            MarkEnd(NoBreakBlock ?? Block);
        }

        internal WhileStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordWhile);

            Condition = Expression.ParseTestExpr(this);
            Block     = new BlockStatement(this);
            if (MaybeEat(TokenType.KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + "while " + Condition + " " + Block;
            if (NoBreakBlock != null) {
                c = c + " nobreak " + NoBreakBlock;
            }

            return c;
        }
    }
}