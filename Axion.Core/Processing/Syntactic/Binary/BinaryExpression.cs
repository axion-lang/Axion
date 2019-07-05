using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Binary {
    /// <summary>
    ///     <c>
    ///         binary_infix_expr:
    ///             expr OPERATOR expr;
    ///     </c>
    /// </summary>
    public class BinaryExpression : LeftRightExpression {
        public readonly Token Operator;

        public BinaryExpression(
            Expression parent,
            Expression left,
            Token      op,
            Expression right
        ) : base(parent) {
            MarkStart(Left = left);
            Operator = op;
            MarkEnd(Right = right);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " ", Operator.Value, " ", Right);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (Spec.CSharp.BinaryOperators.TryGetValue(Operator.Type, out string op)) {
                c.Write("(", Left, ") ", op);
            }
            else if (Operator.Is(OpIn)) {
                // in (list1 or|and list2)
                if (Right is ParenthesizedExpression paren
                 && paren.Value is BinaryExpression collections
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
                c.Write("(", Left, ") ", Operator.Value);
            }

            c.Write(" (", Right, ")");
        }
    }
}