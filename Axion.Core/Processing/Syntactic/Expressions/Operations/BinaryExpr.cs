using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;

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
            set => left = Bind(value);
        }

        private Expr right;

        public Expr Right {
            get => right;
            set => right = Bind(value);
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
    }
}