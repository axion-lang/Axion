using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AugmentedAssignStatement : Statement {
        [JsonProperty]
        internal OperatorToken Operator { get; }

        private Expression left;

        [JsonProperty]
        internal Expression Left {
            get => left;
            set {
                value.Parent = this;
                left         = value;
            }
        }

        private Expression right;

        [JsonProperty]
        internal Expression Right {
            get => right;
            set {
                value.Parent = this;
                right        = value;
            }
        }

        public AugmentedAssignStatement(Expression left, OperatorToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));
        }
    }
}