using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class SliceExpression : Expression {
        private Expression start;

        internal Expression Start {
            get => start;
            set => SetNode(ref start, value);
        }

        private Expression stop;

        public Expression Stop {
            get => stop;
            set => SetNode(ref stop, value);
        }

        private Expression step;

        public Expression Step {
            get => step;
            set => SetNode(ref step, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public SliceExpression(Expression start, Expression stop, Expression step = null) {
            Start = start;
            Stop  = stop;
            Step  = step;

            MarkPosition(start ?? stop ?? step, step ?? stop ?? start);
        }
    }
}