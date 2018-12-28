using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Newtonsoft.Json;
using MSAst = System.Linq.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AugmentedAssignStatement : Statement {
        [JsonProperty]
        internal Expression Left { get; }

        [JsonProperty]
        internal OperatorToken Operator { get; }

        [JsonProperty]
        internal Expression Right { get; }

        public AugmentedAssignStatement(Expression left, OperatorToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));
        }
    }
}