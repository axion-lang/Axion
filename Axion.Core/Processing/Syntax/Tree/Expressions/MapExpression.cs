using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    [JsonObject]
    public class MapExpression : Expression {
        private SliceExpression[] expressions;

        internal MapExpression(SliceExpression[] expressions, Position start, Position end) : base(start, end) {
            Expressions = expressions;
        }

        [JsonProperty]
        internal SliceExpression[] Expressions {
            get => expressions;
            set {
                expressions = value;
                foreach (SliceExpression expr in expressions) {
                    expr.Parent = this;
                }
            }
        }
    }
}