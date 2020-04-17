namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         slice-expr:
    ///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
    ///     </c>
    /// </summary>
    public class SliceExpr : Expr {
        private Expr? from;

        public Expr? From {
            get => from;
            set => from = BindNullable(value);
        }

        private Expr? to;

        public Expr? To {
            get => to;
            set => to = BindNullable(value);
        }

        private Expr? step;

        public Expr? Step {
            get => step;
            set => step = BindNullable(value);
        }

        public SliceExpr(Expr parent) : base(parent) { }
    }
}