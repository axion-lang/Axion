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
            SyntaxTreeNode parent,
            Expression     left,
            OperatorToken  op,
            Expression     right
        ) : base(parent) {
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
            null,
            left,
            new OperatorToken(opType),
            right
        ) { }

        public BinaryOperationExpression(
            Expression left,
            string     op,
            Expression right
        ) : this(
            null,
            left,
            new OperatorToken(op),
            right
        ) { }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " ", Operator.Value, " ", Right);
        }

        public override void ToCSharpCode(CodeBuilder c) {
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