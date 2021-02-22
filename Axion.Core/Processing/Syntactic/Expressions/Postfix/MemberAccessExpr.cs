using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         member-expr:
    ///             atom '.' ID;
    ///     </c>
    /// </summary>
    public class MemberAccessExpr : PostfixExpr {
        private Node? target;

        public Node? Target {
            get => target;
            set => target = BindNullable(value);
        }

        private Node? member;

        public Node? Member {
            get => member;
            set => member = BindNullable(value);
        }

        public MemberAccessExpr(Node parent) : base(parent) { }

        public MemberAccessExpr Parse() {
            Target ??= AtomExpr.Parse(this);
            Stream.Eat(Dot);
            Member = AtomExpr.Parse(this);
            return this;
        }

        // TODO: check for accessing prop/field existence
    }
}
