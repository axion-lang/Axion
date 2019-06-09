namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    public abstract class MultipleExpression : Expression {
        private NodeList<Expression> expressions;

        public NodeList<Expression> Expressions {
            get => expressions;
            set => SetNode(ref expressions, value);
        }

        protected MultipleExpression(AstNode parent) : base(parent) { }

        protected MultipleExpression(NodeList<Expression> exprs) {
            Expressions = exprs ?? new NodeList<Expression>(this);
        }
    }
}