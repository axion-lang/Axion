using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         slice-expr:
    ///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
    ///     </c>
    /// </summary>
    public class SliceExpr : Expr {
        private Expr from;

        internal Expr From {
            get => from;
            set => from = BindNode(value);
        }

        private Expr to;

        public Expr To {
            get => to;
            set => to = BindNode(value);
        }

        private Expr step;

        public Expr Step {
            get => step;
            set => step = BindNode(value);
        }

        public SliceExpr(
            Expr? parent = null,
            Expr? from   = null,
            Expr? to     = null,
            Expr? step   = null
        ) : base(
            parent
         ?? GetParentFromChildren(from, to, step)
        ) {
            From = from;
            To   = to;
            Step = step;

            MarkPosition(from ?? to ?? step, step ?? to ?? from);
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(
                "[", From, ":", To,
                ":", Step, "]"
            );
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(
                ".Slice(", From, ", ", To,
                ", ", Step, ")"
            );
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }
    }
}