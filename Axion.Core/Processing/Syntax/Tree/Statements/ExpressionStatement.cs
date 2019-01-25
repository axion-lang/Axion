using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ExpressionStatement : Statement {
        private Expression expression;

        [JsonProperty]
        internal Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        internal ExpressionStatement(Expression expression) : base(expression) {
            Expression = expression;
        }
    }
}