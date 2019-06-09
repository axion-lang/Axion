using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         break_stmt:
    ///             'break' [name]
    ///     </c>
    /// </summary>
    public class BreakExpression : Expression {
        private SimpleNameExpression loopName;

        public SimpleNameExpression LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal BreakExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.KeywordBreak);
            if (MaybeEat(TokenType.Identifier)) {
                LoopName = new SimpleNameExpression(this);
            }

            MarkEnd();

            if (!Ast.InLoop) {
                Unit.Blame(BlameType.BreakIsOutsideLoop, this);
            }
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public BreakExpression(SimpleNameExpression loopName = null) {
            LoopName = loopName;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("break");
            if (LoopName != null) {
                c.Write(" ", LoopName);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (LoopName == null) {
                c.Write("break;");
                return;
            }

            Unit.ReportError(
                "'break' statement with loop name is not implemented in C#.",
                LoopName
            );
        }
    }
}