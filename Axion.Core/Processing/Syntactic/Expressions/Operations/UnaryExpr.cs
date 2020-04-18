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
            set => val = Bind(value);
        }

        public OperatorToken Operator { get; }

        public UnaryExpr(Expr? parent = null, OperatorToken? op = null, Expr? value = null) : base(
            parent ?? GetParentFromChildren(value)
        ) {
            Operator = op;
            Value    = value;
            MarkStart(Operator);
            MarkEnd(Value);
        }

        public UnaryExpr(Expr? parent = null, TokenType opType = TokenType.None, Expr? value = null)
            : base(parent ?? GetParentFromChildren(value)) {
            Operator = new OperatorToken(Source, tokenType: opType);
            Value    = value;
            MarkStart(Operator);
            MarkEnd(Value);
        }
    }
}
