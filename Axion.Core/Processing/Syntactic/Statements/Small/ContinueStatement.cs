using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         continue_stmt ::=
    ///             'continue' [name]
    ///     </c>
    /// </summary>
    public class ContinueStatement : Statement {
        private NameExpression loopName;

        public NameExpression LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        public ContinueStatement(NameExpression loopName = null) {
            LoopName = loopName;
        }

        internal ContinueStatement(SyntaxTreeNode parent) {
            Parent = parent;
            
            StartNode(TokenType.KeywordContinue);
            if (MaybeEat(TokenType.Identifier)) {
                LoopName = new NameExpression(this, true);
            }
            MarkEnd(Token);
            
            if (!Ast.InLoop) {
                Unit.Blame(BlameType.ContinueIsOutsideLoop, this);
            }
            else if (Ast.InFinally && !Ast.InFinallyLoop) {
                Unit.Blame(BlameType.ContinueNotSupportedInsideFinally, this);
            }
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c += "continue";
            if (LoopName != null) {
                c = c + " " + LoopName;
            }

            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            if (LoopName == null) {
                return c + "continue;";
            }

            Unit.ReportError(
                "'continue' statement with loop name is not implemented in C#.",
                LoopName
            );
            return c;
        }
    }
}