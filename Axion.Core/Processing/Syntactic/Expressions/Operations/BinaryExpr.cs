using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         binary_expr:
    ///             expr OPERATOR expr;
    ///     </c>
    /// </summary>
    public class BinaryExpr : Expr {
        private Expr left;

        public Expr Left {
            get => left;
            set => SetNode(ref left, value);
        }

        private Expr right;

        public Expr Right {
            get => right;
            set => SetNode(ref right, value);
        }

        public Token Operator { get; }

        public BinaryExpr(
            Expr  parent = null,
            Expr  left   = null,
            Token op     = null,
            Expr  right  = null
        ) : base(parent) {
            MarkStart(Left = left);
            Operator = op;
            MarkEnd(Right = right);
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Left, " ", Operator.Value, " ", Right);
        }

        public override void ToCSharp(CodeWriter c) {
            if (Operator.Is(OpPower)) {
                c.Write("Math.Pow(", Left, ", ", Right, ")");
            }
            else if (Operator.Is(OpIn)) {
                // in (list1 or|and list2)
                if (Right is ParenthesizedExpr paren
                 && paren.Value is BinaryExpr collections
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
            }
            else {
                c.Write(Left, " ", Operator.Value, " ", Right);
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Left, " ", Operator.Value, " ", Right);
        }
    }
}