using Axion.Core.Processing.Syntax.Tree.Comprehensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class GeneratorExpression : Expression {
        private Expression iterable;

        private ComprehensionIterator[] comprehensions;

        public GeneratorExpression(Expression iterable, ComprehensionIterator[] comprehensions) {
            Iterable       = iterable;
            Comprehensions = comprehensions;
        }

        [JsonProperty]
        internal Expression Iterable {
            get => iterable;
            set {
                value.Parent = this;
                iterable     = value;
            }
        }

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
    }
}