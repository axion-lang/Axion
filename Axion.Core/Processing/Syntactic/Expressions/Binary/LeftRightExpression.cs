namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    public abstract class LeftRightExpression : Expression {
        private Expression left;

        public Expression Left {
            get => left;
            set => SetNode(ref left, value);
        }

        private Expression right;

        public Expression Right {
            get => right;
            set => SetNode(ref right, value);
        }
    }
}