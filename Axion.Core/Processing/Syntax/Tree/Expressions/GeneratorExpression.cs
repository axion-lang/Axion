using Axion.Core.Processing.Syntax.Tree.Comprehensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class GeneratorExpression : Expression {
        [JsonProperty]
        internal Expression Iterable { get; }

        [JsonProperty]
        internal ComprehensionIterator[] Comprehensions { get; }

        public GeneratorExpression(Expression iterable, ComprehensionIterator[] comprehensions) {
            Iterable       = iterable;
            Comprehensions = comprehensions;
        }
    }
}