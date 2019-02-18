using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class ExpressionStatement : Statement {
        private Expression expression;

        internal ExpressionStatement(Expression expression) : base(expression) {
            Expression = expression;
        }

        [JsonProperty]
        public Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }
    }
}