namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class SliceExpression : Expression {
        public Expression Start { get; }

        public Expression Stop { get; }

        public Expression Step { get; }

        public SliceExpression(Expression start, Expression stop, Expression step) {
            Start = start;
            Stop  = stop;
            Step  = step;

            MarkStart(start ?? stop ?? step);
            MarkEnd(step ?? stop ?? start);
        }

        public SliceExpression(Expression start, Expression stop, Expression step, Position startPos, Position endPos)
            : base(startPos, endPos) {
            Start = start;
            Stop  = stop;
            Step  = step;
        }
    }
}