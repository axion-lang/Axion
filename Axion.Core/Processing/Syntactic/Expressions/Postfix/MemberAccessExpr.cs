using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         member-expr:
    ///             atom '.' ID;
    ///     </c>
    /// </summary>
    public class MemberAccessExpr : PostfixExpr {
        private Expr? target;

        public Expr? Target {
            get => target;
            set => target = BindNullable(value);
        }

        private Expr? member;

        public Expr? Member {
            get => member;
            set => member = BindNullable(value);
        }

        public MemberAccessExpr(Node parent) : base(parent) { }

        public MemberAccessExpr Parse() {
            SetSpan(
                () => {
                    Target ??= AtomExpr.Parse(this);
                    Stream.Eat(Dot);
                    Member = AtomExpr.Parse(this);
                }
            );
            return this;
        }

        // TODO: check for accessing prop/field existence
    }
}
