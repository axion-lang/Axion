using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parenthesis_expr:
    ///             '(' ')'
    ///             | yield_expr
    ///             | test_list
    ///             | generator_expr
    ///     </c>
    /// </summary>
    public class ParenthesizedExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public override TypeName ValueType => Value.ValueType;

        internal ParenthesizedExpression(Expression value) {
            Parent = value.Parent;
            Value  = value;
            MarkPosition(value);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(", Value, ")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(", Value, ")");
        }
    }
}