using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class GeneratorExpression : Expression {
        private Expression comprehension;

        [NotNull]
        public Expression Comprehension {
            get => comprehension;
            set => SetNode(ref comprehension, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public GeneratorExpression([NotNull] Expression comprehension) {
            Comprehension = comprehension;
        }
        
        public GeneratorExpression([NotNull] SyntaxTreeNode parent, Expression target) {
            Parent = parent;
            Comprehension = new ForComprehension(target);
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c;
        }
    }
}