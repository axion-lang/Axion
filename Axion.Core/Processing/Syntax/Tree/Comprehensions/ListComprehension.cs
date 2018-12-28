using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Comprehensions {
    public abstract class Comprehension : Expression {
        public abstract IList<ComprehensionIterator> Iterators { get; }
        public abstract string                       NodeName  { get; }
    }

    public sealed class ListComprehension : Comprehension {
        public ListComprehension(Expression item, IList<ComprehensionIterator> iterators) {
            Item      = item;
            Iterators = iterators;
        }

        public ListComprehension(Expression item, IList<ComprehensionIterator> iterators, Position start, Position end) {
            Item      = item;
            Iterators = iterators;
            MarkPosition(start, end);
        }

        public Expression Item { get; }

        public override IList<ComprehensionIterator> Iterators { get; }

        public override string NodeName => "list comprehension";
    }

    public sealed class SetComprehension : Comprehension {
        public SetComprehension(Expression item, IList<ComprehensionIterator> iterators) {
            Item      = item;
            Iterators = iterators;
        }

        public SetComprehension(Expression item, IList<ComprehensionIterator> iterators, Position start, Position end) {
            Item      = item;
            Iterators = iterators;
            MarkPosition(start, end);
        }

        public Expression Item { get; }

        public override IList<ComprehensionIterator> Iterators { get; }

        public override string NodeName => "set comprehension";
    }

    public sealed class MapComprehension : Comprehension {
        public MapComprehension(Expression key, Expression value, ComprehensionIterator[] iterators) {
            Key       = key;
            Value     = value;
            Iterators = iterators;
        }

        public MapComprehension(Expression key, Expression value, ComprehensionIterator[] iterators, Position start, Position end) {
            Key       = key;
            Value     = value;
            Iterators = iterators;
            MarkPosition(start, end);
        }

        public Expression Key { get; }

        public Expression Value { get; }

        public override IList<ComprehensionIterator> Iterators { get; }

        public override string NodeName => "map comprehension";
    }
}