using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         break_stmt:
    ///             'break' [name]
    ///     </c>
    /// </summary>
    public class BreakStatement : Statement {
        private NameExpression? loopName;

        public NameExpression? LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="BreakStatement"/> from tokens.
        /// </summary>
        internal BreakStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordBreak);
            if (MaybeEat(TokenType.Identifier)) {
                LoopName = new NameExpression(this, true);
            }

            MarkEnd(Token);

            if (!Ast.InLoop) {
                Unit.Blame(BlameType.BreakIsOutsideLoop, this);
            }
        }

        /// <summary>
        ///     Constructs plain <see cref="BreakStatement"/> without position in source.
        /// </summary>
        public BreakStatement(NameExpression? loopName = null) {
            LoopName = loopName;
        }

        #endregion

        #region Code converters

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

        #endregion
    }
}