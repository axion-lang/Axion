using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    public class MultipleExpression<T> : Expression where T : Expression {
        private T[] expressions;

        protected MultipleExpression(Position start, Position end) : base(start, end) {
        }

        protected MultipleExpression() {
        }

        [JsonProperty]
        internal T[] Expressions {
            get => expressions;
            set {
                expressions = value;
                foreach (T expr in expressions) {
                    expr.Parent = this;
                }
            }
        }
    }
}