using System;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class LambdaExpression : Expression {
        private FunctionDefinition function;

        [JsonProperty]
        internal FunctionDefinition Function {
            get => function;
            set {
                value.Parent = this;
                function     = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public LambdaExpression(FunctionDefinition function) {
            Function = function ?? throw new ArgumentNullException(nameof(function));
        }
    }
}