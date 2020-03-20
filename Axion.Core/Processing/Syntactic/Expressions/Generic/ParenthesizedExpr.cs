using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic {
    /// <summary>
    ///     <c>
    ///         parenthesis-expr:
    ///             '(' expr ')';
    ///     </c>
    /// </summary>
    public class ParenthesizedExpr<T> : Multiple<T> where T : Expr {
        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Value.ValueType;

        internal ParenthesizedExpr(Expr value) : base(value.Parent) {
            Value = value;
            MarkPosition(Value);
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