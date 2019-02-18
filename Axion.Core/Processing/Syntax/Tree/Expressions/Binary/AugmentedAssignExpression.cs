using System;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class AugmentedAssignExpression : LeftRightExpression {
        [JsonProperty]
        internal SymbolToken Operator { get; }

        public AugmentedAssignExpression(Expression left, SymbolToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + " " + Operator.Value + " " + Right;
        }
    }
}