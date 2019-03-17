using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class BreakStatement : Statement {
        private Expression loopName;

        public Expression LoopName {
            get => loopName;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                loopName = value;
            }
        }

        internal BreakStatement(
            Token      startToken,
            Expression loopName = null
        ) : base(startToken) {
            LoopName = loopName;

            MarkEnd((SpannedRegion) loopName ?? startToken);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c += "break";
            if (LoopName != null) {
                c = c + " " + LoopName;
            }

            return c;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            if (LoopName != null) {
                SourceUnit.ReportError(
                    "'break' statement with loop name is not implemented in C#.",
                    LoopName
                );
                return c;
            }

            return c + "break;";
        }
    }
}