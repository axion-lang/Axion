using System.Collections.Generic;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions {
    public interface IComprehension {
        IList<ComprehensionIterator> Iterators { get; }
    }

    public sealed class ListComprehension : Expression, IComprehension {
        public ListComprehension(
            Expression                   item,
            IList<ComprehensionIterator> iterators,
            Position                     start,
            Position                     end
        ) {
            Item      = item;
            Iterators = iterators;
            MarkPosition(start, end);
        }

        public Expression Item { get; }

        public IList<ComprehensionIterator> Iterators { get; }
    }

    public sealed class SetComprehension : Expression, IComprehension {
        public SetComprehension(
            Expression                   item,
            IList<ComprehensionIterator> iterators,
            Position                     start,
            Position                     end
        ) {
            Item      = item;
            Iterators = iterators;
            MarkPosition(start, end);
        }

        public Expression Item { get; }

        public IList<ComprehensionIterator> Iterators { get; }
    }

    public sealed class MapComprehension : Expression, IComprehension {
        public MapComprehension(
            Expression              key,
            Expression              value,
            ComprehensionIterator[] iterators,
            Position                start,
            Position                end
        ) {
            Key       = key;
            Value     = value;
            Iterators = iterators;
            MarkPosition(start, end);
        }

        public Expression Key { get; }

        public Expression Value { get; }

        public IList<ComprehensionIterator> Iterators { get; }
    }
}