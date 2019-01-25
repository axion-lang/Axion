using System;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
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

        public LambdaExpression(FunctionDefinition function) {
            Function = function ?? throw new ArgumentNullException(nameof(function));
        }
    }
}