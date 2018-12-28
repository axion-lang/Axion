using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class IndexExpression : Expression {
        [JsonProperty]
        internal Expression Target { get; }

        [JsonProperty]
        internal Expression Index { get; }

        public IndexExpression(Expression target, Expression index) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Index  = index ?? throw new ArgumentNullException(nameof(index));

            MarkStart(target);
            MarkEnd(index);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Target + "[" + Index + "]";
        }
    }
}