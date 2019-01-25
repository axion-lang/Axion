using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class OrExpression : Expression {
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

        public OrExpression(Expression left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }
    }
}