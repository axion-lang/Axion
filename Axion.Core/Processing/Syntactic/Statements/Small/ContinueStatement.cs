using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         continue_stmt:
    ///             'continue' [name]
    ///     </c>
    /// </summary>
    public class ContinueStatement : Statement {
        private SimpleNameExpression? loopName;

        public SimpleNameExpression? LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ContinueStatement"/> from tokens.
        /// </summary>
        internal ContinueStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordContinue);
            if (MaybeEat(TokenType.Identifier)) {
                LoopName = new SimpleNameExpression(this);
            }

            MarkEnd(Token);

            if (!Ast.InLoop) {
                Unit.Blame(BlameType.ContinueIsOutsideLoop, this);
            }
            else if (Ast.InFinally && !Ast.InFinallyLoop) {
                Unit.Blame(BlameType.ContinueNotSupportedInsideFinally, this);
            }
        }

        /// <summary>
        ///     Constructs plain <see cref="ContinueStatement"/> without position in source.
        /// </summary>
        public ContinueStatement(SimpleNameExpression? loopName = null) {
            LoopName = loopName;
        }

        #endregion

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("continue");
            if (LoopName != null) {
                c.Write(" ", LoopName);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            if (LoopName == null) {
                c.Write("continue;");
                return;
            }

            Unit.ReportError(
                "'continue' statement with loop name is not implemented in C#.",
                LoopName
            );
        }

        #endregion
    }
}