using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.TypeNames;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     <c>
    ///         parenthesis_expr:
    ///             '(' expr ')';
    ///     </c>
    /// </summary>
    public class ParenthesizedExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public override TypeName ValueType => Value.ValueType;

        internal ParenthesizedExpression(Expression value) : base(value.Parent) {
            MarkPosition(Value = value);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(", Value, ")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(", Value, ")");
        }
    }
}