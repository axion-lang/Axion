namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public abstract class Expression : SpannedRegion {
        protected Expression() {
        }

        protected Expression(Position start, Position end) {
            MarkPosition(start, end);
        }

        internal virtual string CannotDeleteReason => null;

        internal virtual string CannotAssignReason => null;

        internal virtual string CannotAugAssignReason => null;
    }
}