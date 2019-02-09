using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Comprehensions {
    public class ComprehensionFor : ComprehensionIterator {
        public ComprehensionFor(SpannedRegion start, Expression left, Expression list) {
            Left = left;
            List = list;

            MarkPosition(start, list);
        }

        public Expression Left { get; }

        public Expression List { get; }
    }
}