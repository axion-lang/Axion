using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parenthesis-expr:
    ///             '(' expr ')';
    ///     </c>
    /// </summary>
    public class ParenthesizedExpr : AtomExpr {
        private Expr val = null!;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        public override TypeName ValueType => Value.ValueType;

        internal ParenthesizedExpr(Expr value) : base(value.Parent) {
            Value = value;
        }
    }
}
