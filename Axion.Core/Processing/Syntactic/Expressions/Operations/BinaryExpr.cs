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
        private Expr left = null!;

        public Expr Left {
            get => left;
            set => left = Bind(value);
        }

        private Expr right = null!;

        public Expr Right {
            get => right;
            set => right = Bind(value);
        }

        private Token @operator = null!;

        public Token Operator {
            get => @operator;
            set => @operator = Bind(value);
        }

        public BinaryExpr(Node parent) : base(parent) { }
    }
}
