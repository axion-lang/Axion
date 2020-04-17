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
        private Expr target = null!;

        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        private Expr member = null!;

        public Expr Member {
            get => member;
            set => member = Bind(value);
        }

        public MemberAccessExpr(Expr parent) : base(parent) { }

        public MemberAccessExpr Parse() {
            SetSpan(
                () => {
                    Target ??= AtomExpr.Parse(this);
                    Stream.Eat(OpDot);
                    Member = AtomExpr.Parse(this);
                }
            );
            return this;
        }

        // TODO: check for accessing prop/field existence
    }
}