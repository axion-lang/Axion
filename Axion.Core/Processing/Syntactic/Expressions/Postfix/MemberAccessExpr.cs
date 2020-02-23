using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         member_expr:
    ///             atom '.' ID;
    ///     </c>
    /// </summary>
    public class MemberAccessExpr : Expr {
        private Expr target;

        public Expr Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expr member;

        public Expr Member {
            get => member;
            set => SetNode(ref member, value);
        }

        public MemberAccessExpr(Expr parent = null, Expr target = null) : base(parent) {
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

        public override void ToAxion(CodeWriter c) {
            c.Write(Target, ".", Member);
        }

        public override void ToCSharp(CodeWriter c) {
            ToAxion(c);
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }

        public override void ToPascal(CodeWriter c) {
            ToAxion(c);
        }
    }
}