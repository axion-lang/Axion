using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     <c>
    ///         conditional_expr:
    ///             'if' preglobal_expr block
    ///             {'elif' preglobal_expr block}
    ///             ['else' block];
    ///     </c>
    /// </summary>
    public class ConditionalExpression : Expression {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockExpression thenBlock;

        public BlockExpression ThenBlock {
            get => thenBlock;
            set => SetNode(ref thenBlock, value);
        }

        private BlockExpression elseBlock;

        public BlockExpression ElseBlock {
            get => elseBlock;
            set => SetNode(ref elseBlock, value);
        }

        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal ConditionalExpression(Expression parent, bool elseIf = false) {
            Construct(parent, () => {
                if (!elseIf) {
                    Eat(KeywordIf);
                }

                Condition = ParseInfixExpr(this);
                ThenBlock = new BlockExpression(this);

                if (MaybeEat(KeywordElse)) {
                    ElseBlock = new BlockExpression(this);
                }
                else if (MaybeEat(KeywordElif)) {
                    ElseBlock = new BlockExpression(this, new ConditionalExpression(this, true));
                }
                else if (elseIf) {
                    BlameInvalidSyntax(KeywordElse, Peek);
                }
            });
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