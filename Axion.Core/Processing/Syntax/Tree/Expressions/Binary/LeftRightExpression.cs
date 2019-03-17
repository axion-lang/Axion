using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public abstract class LeftRightExpression : Expression {
        private Expression left;

        public Expression Left {
            get => left;
            set {
                value.Parent = this;
                left         = value;
            }
        }

        private Expression right;

        public Expression Right {
            get => right;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                right = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;
    }
}