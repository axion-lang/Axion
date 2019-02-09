using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class MemberExpression : Expression {
        private Expression target;

        private Expression name;

        public MemberExpression(Expression target, NameExpression identifier) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Name   = identifier ?? throw new ArgumentNullException(nameof(identifier));

            MarkPosition(target, identifier);
        }

        [JsonProperty]
        internal Expression Target {
            get => target;
            set {
                value.Parent = this;
                target       = value;
            }
        }

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        public override string ToString() {
            return Target + "." + Name;
        }
    }
}