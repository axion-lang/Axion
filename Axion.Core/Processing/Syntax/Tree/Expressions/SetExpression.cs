using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class SetExpression : Expression {
        private Expression[] expressions;

        [JsonProperty]
        internal Expression[] Expressions {
            get => expressions;
            set {
                expressions = value;
                foreach (Expression expr in expressions) {
                    expr.Parent = this;
                }
            }
        }

        internal SetExpression(Expression[] expressions, Position start, Position end)
            : base(start, end) {
            Expressions = expressions;
        }
    }
}