using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         member_expr:
    ///             atom '.' ID
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

        public MemberAccessExpression(AstNode parent, Expression target) : base(parent) {
            MarkStart(Target = target);
            Eat(Dot);
            MarkEnd(Member = new SimpleNameExpression(this));
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Target, ".", Member);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Target, ".", Member);
        }
    }
}