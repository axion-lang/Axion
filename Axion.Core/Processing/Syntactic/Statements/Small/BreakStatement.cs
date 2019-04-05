using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         break_stmt ::=
    ///             'break' [name]
    ///     </c>
    /// </summary>
    public class BreakStatement : Statement {
        private Expression loopName;

        public Expression LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        public BreakStatement(Expression loopName = null) {
            LoopName = loopName;
        }

        internal BreakStatement(SyntaxTreeNode parent) {
            Parent = parent;
            
            StartNode(TokenType.KeywordBreak);
            if (MaybeEat(TokenType.Identifier)) {
                LoopName = new NameExpression(this, true);
            }
            MarkEnd(Token);
            
            if (!Ast.InLoop) {
                Unit.Blame(BlameType.BreakIsOutsideLoop, this);
            }
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c += "break";
            if (LoopName != null) {
                c = c + " " + LoopName;
            }

            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            if (LoopName == null) {
                return c + "break;";
            }

            Unit.ReportError(
                "'break' statement with loop name is not implemented in C#.",
                LoopName
            );
            return c;
        }
    }
}