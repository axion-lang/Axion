using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class SliceExpression : Expression {
        private Expression start;

        [JsonProperty]
        internal Expression Start {
            get => start;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                start = value;
            }
        }

        private Expression stop;

        [JsonProperty]
        internal Expression Stop {
            get => stop;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                stop = value;
            }
        }

        private Expression step;

        [JsonProperty]
        internal Expression Step {
            get => step;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                step = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public SliceExpression(Expression start, Expression stop, Expression step) {
            Start = start;
            Stop  = stop;
            Step  = step;

            MarkStart(start ?? stop ?? step);
            MarkEnd(step ?? stop ?? start);
        }
    }
}