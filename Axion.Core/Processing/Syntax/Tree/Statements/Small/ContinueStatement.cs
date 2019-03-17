using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class ContinueStatement : Statement {
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

        internal ContinueStatement(
            Token      startToken,
            Expression loopName = null
        ) : base(startToken) {
            LoopName = loopName;

            MarkEnd((SpannedRegion) loopName ?? startToken);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c += "continue";
            if (LoopName != null) {
                c = c + " " + LoopName;
            }

            return c;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            if (LoopName != null) {
                SourceUnit.ReportError(
                    "'continue' statement with loop name is not implemented in C#.",
                    LoopName
                );
                return c;
            }

            return c + "continue;";
        }
    }
}