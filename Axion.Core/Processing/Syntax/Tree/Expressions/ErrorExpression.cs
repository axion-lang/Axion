namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ErrorExpression : Expression {
        internal ErrorExpression(SpannedRegion mark) {
            MarkPosition(mark);
        }

        internal ErrorExpression(Position start, Position end) : base(start, end) {
        }
    }
}