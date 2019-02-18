namespace Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions {
    public class ForComprehension : ComprehensionIterator {
        public ForComprehension(SpannedRegion start, Expression left, Expression list) {
            Left = left;
            List = list;

            MarkPosition(start, list);
        }

        public Expression Left { get; }

        public Expression List { get; }
    }
}