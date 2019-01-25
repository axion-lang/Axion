namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ErrorExpression : Expression {
        internal ErrorExpression(Position start, Position end) : base(start, end) {
        }
    }
}