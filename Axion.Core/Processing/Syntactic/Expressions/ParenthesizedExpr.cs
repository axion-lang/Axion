using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parenthesis_expr:
    ///             '(' expr ')';
    ///     </c>
    /// </summary>
    public class ParenthesizedExpr : Expr {
        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public override TypeName ValueType => Value.ValueType;

        internal ParenthesizedExpr(Expr value) : base(value.Parent) {
            MarkPosition(Value = value);
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("(", Value, ")");
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("(", Value, ")");
        }

        public override void ToPython(CodeWriter c) {
            c.Write("(", Value, ")");
        }

        public override void ToPascal(CodeWriter c) {
            c.Write("(", Value, ")");
        }
    }
}