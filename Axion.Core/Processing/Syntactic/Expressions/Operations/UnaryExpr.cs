using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         unary-expr:
    ///             UNARY-LEFT prefix-expr
    ///             | suffix-expr UNARY-RIGHT;
    ///     </c>
    /// </summary>
    public class UnaryExpr : PostfixExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => val = BindNode(value);
        }

        public OperatorToken Operator { get; }

        public UnaryExpr(
            Expr?          parent = null,
            OperatorToken? op     = null,
            Expr?          value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(value)
        ) {
            Operator = op;
            Value    = value;
            MarkStart(Operator);
            MarkEnd(Value);
        }

        public UnaryExpr(
            Expr?     parent = null,
            TokenType opType = TokenType.None,
            Expr?     value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(value)
        ) {
            Operator = new OperatorToken(Source, tokenType: opType);
            Value    = value;
            MarkStart(Operator);
            MarkEnd(Value);
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