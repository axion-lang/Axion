using System;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class BinaryExpression : Expression {
        [JsonProperty]
        internal OperatorToken Operator { get; }

        private Expression left;

        [JsonProperty]
        internal Expression Left {
            get => left;
            set {
                value.Parent = this;
                left         = value;
            }
        }

        private Expression right;

        [JsonProperty]
        internal Expression Right {
            get => right;
            set {
                value.Parent = this;
                right        = value;
            }
        }

        public BinaryExpression(Expression left, OperatorToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + " " + Operator.Value + " " + Right;
        }
    }
}