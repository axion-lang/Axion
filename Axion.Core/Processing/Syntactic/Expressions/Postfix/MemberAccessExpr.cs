using Axion.Core.Processing.CodeGen;
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
        private Expr target;

        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        private Expr member;

        public Expr Member {
            get => member;
            set => member = Bind(value);
        }

        public MemberAccessExpr(
            Expr? parent = null,
            Expr? target = null
        ) : base(
            parent
         ?? GetParentFromChildren(target)
        ) {
            Target = target;
        }

        public MemberAccessExpr Parse() {
            SetSpan(
                () => {
                    if (Target == null) {
                        Target = AtomExpr.Parse(this);
                    }

                    Stream.Eat(OpDot);
                    Member = AtomExpr.Parse(this);
                }
            );
            return this;
        }

        // TODO: check for accessing prop/field existence

        public override void ToDefault(CodeWriter c) {
            c.Write(Target, ".", Member);
        }
    }
}