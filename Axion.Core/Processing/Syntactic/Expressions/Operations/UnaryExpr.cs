using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         unary_expr:
    ///             UNARY_LEFT prefix_expr
    ///             | suffix_expr UNARY_RIGHT;
    ///     </c>
    /// </summary>
    public class UnaryExpr : Expr {
        public readonly OperatorToken Operator;
        private         Expr          val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public UnaryExpr(
            Expr          parent = null,
            OperatorToken op     = null,
            Expr          expr   = null
        ) : base(parent) {
            MarkStart(Operator = op);
            MarkEnd(Value      = expr);
        }

        public UnaryExpr(
            Expr      parent = null,
            TokenType opType = TokenType.None,
            Expr      expr   = null
        ) : base(parent) {
            MarkStart(Operator = new OperatorToken(Source, tokenType: opType));
            MarkEnd(Value      = expr);
        }

        public override void ToAxion(CodeWriter c) {
            if (Operator.Side == InputSide.Right) {
                c.Write(Operator.Value, " (", Value, ")");
            }
            else {
                c.Write("(", Value, ") ", Operator.Value);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            string op = Operator.Value;
            if (op == "not") {
                op = "!";
            }

            if (Operator.Side == InputSide.Right) {
                c.Write(op, " (", Value, ")");
            }
            else {
                c.Write("(", Value, ") ", op);
            }
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }
    }
}