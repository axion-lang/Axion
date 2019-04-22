namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    public abstract class MultipleExpression<T> : Expression where T : Expression {
        private NodeList<T> expressions;

        public NodeList<T> Expressions {
            get => expressions;
            set => SetNode(ref expressions, value);
        }

        protected MultipleExpression(SyntaxTreeNode parent) : base(parent) { }
        protected MultipleExpression() { }
    }
}