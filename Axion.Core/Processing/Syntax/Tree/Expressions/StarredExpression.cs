using System;
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

        public StarredExpression(Position start, Expression value) {
            Value = value ?? throw new ArgumentNullException(nameof(value));

            MarkStart(start);
            MarkEnd(value);
        }
    }
}