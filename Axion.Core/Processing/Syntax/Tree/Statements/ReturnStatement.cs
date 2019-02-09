using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ReturnStatement : Statement {
        private Expression expression;

        internal ReturnStatement(Expression expression) {
            Expression = expression;
            MarkPosition(expression);
        }

        internal ReturnStatement(Expression expression, SpannedRegion start) {
            Expression = expression;

            MarkStart(start);
            MarkEnd(expression ?? start);
        }

        [JsonProperty]
        internal Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }
    }
}