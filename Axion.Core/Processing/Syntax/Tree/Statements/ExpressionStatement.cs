using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ExpressionStatement : Statement {
        [JsonProperty]
        internal Expression Expression { get; }

        internal ExpressionStatement(Expression expression) : base(expression) {
            Expression = expression;
        }
    }
}