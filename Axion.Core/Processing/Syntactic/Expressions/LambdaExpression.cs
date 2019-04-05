using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class LambdaExpression : Expression {
        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => SetNode(ref returnType, value);
        }

        private NodeList<Parameter> parameters;

        [NotNull]
        public NodeList<Parameter> Parameters {
            get => parameters;
            set => SetNode(ref parameters, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public LambdaExpression(
            [NotNull] BlockStatement block,
            NodeList<Parameter>      parameters = null
        ) {
            Block      = block;
            Parameters = parameters ?? new NodeList<Parameter>(this);
        }
    }
}