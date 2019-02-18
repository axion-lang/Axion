using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Multiple {
    [JsonObject]
    public class MapExpression : MultipleExpression<SliceExpression> {
        internal MapExpression(SliceExpression[] expressions, Position start, Position end) : base(
            start,
            end
        ) {
            Expressions = expressions;
        }
    }
}