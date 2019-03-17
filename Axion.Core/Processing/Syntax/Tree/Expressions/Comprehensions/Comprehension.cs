using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions {
    public abstract class Comprehension : Expression {
        public IList<ComprehensionIterator> Iterators { get; set; }
    }

    public sealed class ListComprehension : Comprehension {
        public Expression Item { get; }

        public ListComprehension(
            Expression                   item,
            IList<ComprehensionIterator> iterators
        ) {
            Item      = item;
            Iterators = iterators;
        }

        public ListComprehension(
            Expression                   item,
            IList<ComprehensionIterator> iterators,
            Token                        start,
            Token                        end
        ) : this(item, iterators) {
            MarkPosition(start, end);
        }
    }

    public sealed class SetComprehension : Comprehension {
        public Expression Item { get; }

        public SetComprehension(
            Expression                   item,
            IList<ComprehensionIterator> iterators
        ) {
            Item      = item;
            Iterators = iterators;
        }

        public SetComprehension(
            Expression                   item,
            IList<ComprehensionIterator> iterators,
            Token                        start,
            Token                        end
        ) : this(item, iterators) {
            MarkPosition(start, end);
        }
    }

    public sealed class MapComprehension : Comprehension {
        public Expression Key   { get; }
        public Expression Value { get; }

        public MapComprehension(
            Expression              key,
            Expression              value,
            ComprehensionIterator[] iterators
        ) {
            Key       = key;
            Value     = value;
            Iterators = iterators;
        }

        public MapComprehension(
            Expression              key,
            Expression              value,
            ComprehensionIterator[] iterators,
            Token                   start,
            Token                   end
        ) : this(key, value, iterators) {
            MarkPosition(start, end);
        }
    }
}