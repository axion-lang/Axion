namespace Axion.Core.Processing.Syntactic.Expressions {
    public class ErrorExpression : Expression {
        public ErrorExpression(SpannedRegion region) {
            MarkPosition(region);
        }
    }
}