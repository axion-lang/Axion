using System;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AugmentedAssignExpression : Expression {
        private Expression left;

        private Expression right;

        public AugmentedAssignExpression(Expression left, SymbolToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));
        }

        [JsonProperty]
        internal SymbolToken Operator { get; }

        [JsonProperty]
        internal Expression Left {
            get => left;
            set {
                value.Parent = this;
                left         = value;
            }
        }

        [JsonProperty]
        internal Expression Right {
            get => right;
            set {
                value.Parent = this;
                right        = value;
            }
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + " " + Operator.Value + " " + Right;
        }
    }
}