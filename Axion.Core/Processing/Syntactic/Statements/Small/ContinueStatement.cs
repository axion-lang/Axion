using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

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

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ContinueStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordContinue);
            if (MaybeEat(Identifier)) {
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
        ///     Constructs without position in source.
        /// </summary>
        public ContinueStatement(SimpleNameExpression? loopName = null) {
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