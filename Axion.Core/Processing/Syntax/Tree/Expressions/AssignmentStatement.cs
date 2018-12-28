using System;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AssignmentStatement : Statement {
        [JsonProperty]
        internal Expression[] Left { get; }

        [JsonProperty]
        internal Expression Right { get; }

        public AssignmentStatement(Expression[] left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }
    }
}