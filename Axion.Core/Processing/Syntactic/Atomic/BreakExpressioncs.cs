using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
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
        internal BreakExpression(Expression parent) {
            Construct(parent, () => {
                Eat(KeywordBreak);
                if (MaybeEat(Identifier)) {
                    LoopName = new SimpleNameExpression(this);
                }
            });
            if (!ParentBlock.InLoop) {
                Unit.Blame(BlameType.BreakIsOutsideLoop, this);
            }
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