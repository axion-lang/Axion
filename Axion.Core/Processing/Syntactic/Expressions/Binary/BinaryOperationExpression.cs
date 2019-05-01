using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    /// <summary>
    ///     <c>
    ///         binary_operation_expr:
    ///             expr OPERATOR expr;
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

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " ", Operator.Value, " ", Right);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (Spec.CSharp.BinaryOperators.TryGetValue(Operator.Type, out string op)) {
                c.Write(Left, " ", op);
            }
            else if (Operator.Is(OpIn)) {
                // in (c1 or|and c2)
                if (Right is ParenthesizedExpression paren
                    && paren.Value is BinaryOperationExpression collections
                    && collections.Operator.Is(OpAnd, OpOr)) {
                    c.Write(
                        collections.Right,
                        ".Contains(",
                        Left,
                        ") ",
                        Spec.CSharp.BinaryOperators[collections.Operator.Type],
                        " ",
                        collections.Left,
                        ".Contains(",
                        Left,
                        ")"
                    );
                }
                else {
                    c.Write(Right, ".Contains(", Left, ")");
                }

                return;
            }
            else {
                Unit.ReportError("This operator is not implemented in C#.", Operator);
                c.Write(Left, " ", Operator.Value);
            }

            c.Write(" ", Right);
        }
    }
}