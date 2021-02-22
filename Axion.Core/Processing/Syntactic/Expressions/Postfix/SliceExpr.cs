namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         slice-expr:
    ///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
    ///     </c>
    /// </summary>
    public class SliceExpr : Node {
        private Node? from;

        public Node? From {
            get => from;
            set => from = BindNullable(value);
        }

        private Node? to;

        public Node? To {
            get => to;
            set => to = BindNullable(value);
        }

        private Node? step;

        public Node? Step {
            get => step;
            set => step = BindNullable(value);
        }

        public SliceExpr(Node parent) : base(parent) { }
    }
}
