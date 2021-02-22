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
        private Node? left;

        public Node? Left {
            get => left;
            set => left = BindNullable(value);
        }

        private Node? right;

        public Node? Right {
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
