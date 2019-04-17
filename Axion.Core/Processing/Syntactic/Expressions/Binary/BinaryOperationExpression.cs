using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    /// <summary>
    ///     <c>
    ///         bin_expr:
    ///             expr OPERATOR expr
    ///     </c>
    /// </summary>
    public class BinaryOperationExpression : LeftRightExpression {
        public readonly OperatorToken Operator;

        public BinaryOperationExpression(
            Expression    left,
            OperatorToken op,
            Expression    right
        ) {
            Left     = left;
            Operator = op;
            Right    = right;

            MarkPosition(Left, Right);
        }

        public BinaryOperationExpression(
            Expression left,
            TokenType  opType,
            Expression right
        ) : this(
            left,
            new OperatorToken(opType),
            right
        ) { }

        public BinaryOperationExpression(
            Expression left,
            string     op,
            Expression right
        ) : this(
            left,
            new OperatorToken(op),
            right
        ) { }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " ", Operator.Value, " ", Right);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Left, " ");
            if (Spec.CSharp.BinaryOperators.TryGetValue(Operator.Type, out string op)) {
                c.Write(op);
            }
            else {
                Unit.ReportError("This operator is not implemented in C#.", Operator);
                c.Write(Operator.Value);
            }

            c.Write(" ", Right);
        }
    }
}