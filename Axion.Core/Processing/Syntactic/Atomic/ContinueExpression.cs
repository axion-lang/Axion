using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         continue_expr:
    ///             'continue' [name]
    ///     </c>
    /// </summary>
    public class ContinueExpression : Expression {
        private SimpleNameExpression loopName;

        public SimpleNameExpression LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ContinueExpression(Expression parent) {
            Construct(parent, () => {
                Eat(KeywordContinue);
                if (MaybeEat(Identifier)) {
                    LoopName = new SimpleNameExpression(this);
                }
            });

            if (!ParentBlock.InLoop) {
                Unit.Blame(BlameType.ContinueIsOutsideLoop, this);
            }
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("continue");
            if (LoopName != null) {
                c.Write(" ", LoopName);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (LoopName == null) {
                c.Write("continue;");
                return;
            }

            Unit.ReportError(
                "'continue' statement with loop name is not implemented in C#.",
                LoopName
            );
        }
    }
}