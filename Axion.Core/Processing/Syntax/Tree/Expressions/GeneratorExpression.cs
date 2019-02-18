using Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class GeneratorExpression : Expression {
        private Expression iterable;

        [JsonProperty]
        internal Expression Iterable {
            get => iterable;
            set {
                value.Parent = this;
                iterable     = value;
            }
        }

        private ComprehensionIterator[] comprehensions;

        [JsonProperty]
        internal ComprehensionIterator[] Comprehensions {
            get => comprehensions;
            set {
                comprehensions = value;
                foreach (ComprehensionIterator comp in comprehensions) {
                    comp.Parent = this;
                }
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public GeneratorExpression(Expression iterable, ComprehensionIterator[] comprehensions) {
            Iterable       = iterable;
            Comprehensions = comprehensions;
        }
    }
}