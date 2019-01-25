using System;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AssignmentStatement : Statement {
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

        public AssignmentStatement(Expression[] left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }
    }
}