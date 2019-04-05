using Axion.Core.Processing.CodeGen;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class MemberExpression : Expression {
        private Expression target;

        [NotNull]
        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expression member;

        [NotNull]
        public Expression Member {
            get => member;
            set => SetNode(ref member, value);
        }

        internal override string CannotAssignReason =>
            Target.CannotAssignReason ?? Member.CannotAssignReason;

        public MemberExpression([NotNull] Expression target, [NotNull] Expression member) {
            Target = target;
            Member = member;

            MarkPosition(target, member);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Target + "." + Member;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + Target + "." + Member;
        }
    }
}