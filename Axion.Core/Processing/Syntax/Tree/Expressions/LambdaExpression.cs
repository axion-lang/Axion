using System;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class LambdaExpression : Expression {
        [JsonProperty]
        internal FunctionDefinition Function { get; }

        public LambdaExpression(FunctionDefinition function) {
            Function = function ?? throw new ArgumentNullException(nameof(function));
        }
    }
}