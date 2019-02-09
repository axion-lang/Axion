using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AssignmentExpression : Expression {
        private Expression[] left;

        private Expression right;

        public AssignmentExpression(Expression[] left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <summary>
        ///     left.len = 1 for x = 4
        ///     left.len = 3 for x = y = z = 4
        /// </summary>
        [JsonProperty]
        internal Expression[] Left {
            get => left;
            set {
                left = value;
                foreach (Expression expr in left) {
                    expr.Parent = this;
                }
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
            return Left + " = " + Right;
        }
    }
}