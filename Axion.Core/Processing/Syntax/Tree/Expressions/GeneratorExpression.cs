using Axion.Core.Processing.Syntax.Tree.Comprehensions;
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
                foreach (ComprehensionIterator compr in comprehensions) {
                    compr.Parent = this;
                }
            }
        }

        public GeneratorExpression(Expression iterable, ComprehensionIterator[] comprehensions) {
            Iterable       = iterable;
            Comprehensions = comprehensions;
        }
    }
}