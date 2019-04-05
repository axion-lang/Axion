using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class StarredExpression : Expression {
        private Expression val;

        [NotNull]
        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public StarredExpression(Token start, [NotNull] Expression value) {
            Value = value;

            MarkPosition(start, value);
        }
    }
}