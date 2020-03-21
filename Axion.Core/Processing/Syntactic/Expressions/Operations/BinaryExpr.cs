using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         binary-expr:
    ///             expr OPERATOR expr;
    ///     </c>
    /// </summary>
    public class BinaryExpr : InfixExpr {
        private Expr left;

        public Expr Left {
            get => left;
            set => left = BindNode(value);
        }

        private Expr right;

        public Expr Right {
            get => right;
            set => right = BindNode(value);
        }

        public Token Operator { get; }

        public BinaryExpr(
            Expr?  parent = null,
            Expr?  left   = null,
            Token? op     = null,
            Expr?  right  = null
        ) : base(
            parent
         ?? GetParentFromChildren(left, right)
        ) {
            Left     = left;
            Operator = op;
            Right    = right;
            MarkStart(Left);
            MarkEnd(Right);
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(
                Left, " ", Operator.Value, " ",
                Right
            );
        }

        public override void ToCSharp(CodeWriter c) {
            if (Operator.Is(OpPower)) {
                c.Write(
                    "Math.Pow(", Left, ", ", Right,
                    ")"
                );
            }
            else if (Operator.Is(OpIn)) {
                // in (list1 or|and list2)
                if (Right is ParenthesizedExpr<Expr> paren
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
                if (!Spec.CSharp.BinaryOperators.TryGetValue(Operator.Type, out string op)) {
                    op = Operator.Value;
                }
                c.Write(
                    Left, " ", op, " ",
                    Right
                );
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write(
                Left, " ", Operator.Value, " ",
                Right
            );
        }
    }
}