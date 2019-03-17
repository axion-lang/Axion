using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class ExpressionStatement : Statement {
        private Expression expression;

        [JsonProperty]
        public Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        internal ExpressionStatement(Expression expression) {
            Expression = expression;
            MarkPosition(expression);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Expression;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Expression + ";";
        }
    }
}