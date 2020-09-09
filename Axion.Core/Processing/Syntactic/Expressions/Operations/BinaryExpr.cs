using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         binary-expr:
    ///             infix OPERATOR infix;
    ///     </c>
    /// </summary>
    public class BinaryExpr : InfixExpr {
        private Expr? left;

        public Expr? Left {
            get => left;
            set => left = BindNullable(value);
        }

        private Expr? right;

        public Expr? Right {
            get => right;
            set => right = BindNullable(value);
        }

        private Token? @operator;

        public Token? Operator {
            get => @operator;
            set => @operator = BindNullable(value);
        }

        public BinaryExpr(Node parent) : base(parent) { }
    }
}
