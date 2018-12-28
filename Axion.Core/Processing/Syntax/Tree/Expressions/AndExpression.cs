using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AndExpression : Expression {
        [JsonProperty]
        internal Expression Left { get; }

        [JsonProperty]
        internal Expression Right { get; }

        public AndExpression(Expression left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + " and " + Right;
        }
    }
}