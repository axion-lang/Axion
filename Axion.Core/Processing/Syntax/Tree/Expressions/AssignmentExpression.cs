using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AssignmentExpression : Expression {
        private Expression[] left;

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

        private Expression right;

        [JsonProperty]
        internal Expression Right {
            get => right;
            set {
                value.Parent = this;
                right        = value;
            }
        }

        public AssignmentExpression(Expression[] left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }
    }
}