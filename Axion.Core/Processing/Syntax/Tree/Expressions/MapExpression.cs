using System.Collections.Generic;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    [JsonObject]
    public class MapExpression : Expression {
        [JsonProperty]
        internal SliceExpression[] Expressions { get; }

        internal MapExpression(List<SliceExpression> expressions, Position start, Position end)
            : base(start, end) {
            Expressions = expressions.ToArray();
        }
    }
}