using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ReturnStatement : Statement {
        [JsonProperty]
        public Expression Expression { get; }

        internal ReturnStatement(Expression expression) {
            Expression = expression;
            MarkPosition(expression);
        }

        internal ReturnStatement(Expression expression, SpannedRegion start) {
            Expression = expression;

            MarkStart(start);
            MarkEnd(expression ?? start);
        }
    }
}