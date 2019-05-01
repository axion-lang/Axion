using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         member_expr:
    ///             primary '.' ID
    ///     </c>
    /// </summary>
    public class MemberAccessExpression : Expression {
        private Expression target;

        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expression member;

        public Expression Member {
            get => member;
            set => SetNode(ref member, value);
        }

        public MemberAccessExpression(SyntaxTreeNode parent, Expression target) : base(parent) {
            Target = target;

            Eat(Dot);
            Member = new SimpleNameExpression(this);

            MarkPosition(Target, Member);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Target, ".", Member);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Target, ".", Member);
        }
    }
}