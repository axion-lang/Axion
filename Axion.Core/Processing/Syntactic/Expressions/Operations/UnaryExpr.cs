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
        private Expr val = null!;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        public OperatorToken Operator { get; set; } = null!;

        public UnaryExpr(Node parent) : base(parent) { }
    }
}
