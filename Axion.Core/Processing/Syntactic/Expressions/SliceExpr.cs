using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         slice_expr:
    ///             [preglobal_expr] ':' [preglobal_expr] [':' [preglobal_expr]];
    ///     </c>
    /// </summary>
    public class SliceExpr : Expr {
        private Expr from;

        internal Expr From {
            get => from;
            set => SetNode(ref from, value);
        }

        private Expr to;

        public Expr To {
            get => to;
            set => SetNode(ref to, value);
        }

        private Expr step;

        public Expr Step {
            get => step;
            set => SetNode(ref step, value);
        }

        public SliceExpr(
            Expr parent = null,
            Expr from   = null,
            Expr to     = null,
            Expr step   = null
        ) : base(parent) {
            From = from;
            To   = to;
            Step = step;

            MarkPosition(from ?? to ?? step, step ?? to ?? from);
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("[", From, ":", To, ":", Step, "]");
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(".Slice(", From, ", ", To, ", ", Step, ")");
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }
    }
}