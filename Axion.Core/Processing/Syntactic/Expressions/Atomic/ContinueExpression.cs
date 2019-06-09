using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
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
        internal ContinueExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.KeywordContinue);
            if (MaybeEat(TokenType.Identifier)) {
                LoopName = new SimpleNameExpression(this);
            }

            MarkEnd();

            if (!Ast.InLoop) {
                Unit.Blame(BlameType.ContinueIsOutsideLoop, this);
            }
            else if (Ast.InFinally && !Ast.InFinallyLoop) {
                Unit.Blame(BlameType.ContinueNotSupportedInsideFinally, this);
            }
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public ContinueExpression(SimpleNameExpression loopName = null) {
            LoopName = loopName;
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