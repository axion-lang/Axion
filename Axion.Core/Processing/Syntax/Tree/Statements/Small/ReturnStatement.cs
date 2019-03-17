using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class ReturnStatement : Statement {
        private Expression expression;

        public Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        internal ReturnStatement(
            Token      startToken,
            Expression expression
        ) : base(startToken) {
            Expression = expression;

            MarkEnd((SpannedRegion) expression ?? startToken);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + "return " + Expression;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + "return " + Expression + ";";
        }
    }
}