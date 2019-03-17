using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class StarredExpression : Expression {
        private Expression value;

        [JsonProperty]
        internal Expression Value {
            get => value;
            set {
                value.Parent = this;
                this.value   = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public StarredExpression(Token start, Expression value) {
            Value = value ?? throw new ArgumentNullException(nameof(value));

            MarkPosition(start, value);
        }
    }
}