using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class SliceExpression : Expression {
        private Expression start;

        private Expression stop;

        private Expression step;

        public SliceExpression(Expression start, Expression stop, Expression step) {
            Start = start;
            Stop  = stop;
            Step  = step;

            MarkStart(start ?? stop ?? step);
            MarkEnd(step ?? stop ?? start);
        }

        [JsonProperty]
        internal Expression Start {
            get => start;
            set {
                start.Parent = this;
                start        = value;
            }
        }

        [JsonProperty]
        internal Expression Stop {
            get => stop;
            set {
                stop.Parent = this;
                stop        = value;
            }
        }

        [JsonProperty]
        internal Expression Step {
            get => step;
            set {
                step.Parent = this;
                step        = value;
            }
        }
    }
}