using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parenthesis-expr:
    ///             '(' expr ')';
    ///     </c>
    /// </summary>
    public class ParenthesizedExpr : AtomExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => val = BindNode(value);
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