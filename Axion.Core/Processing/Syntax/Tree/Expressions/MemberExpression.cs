using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class MemberExpression : Expression {
        private Expression target;

        [JsonProperty]
        internal Expression Target {
            get => target;
            set {
                value.Parent = this;
                target       = value;
            }
        }

        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        internal override string CannotAssignReason =>
            Target.CannotAssignReason ?? Name.CannotAssignReason;

        public MemberExpression(Expression target, NameExpression identifier) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Name   = identifier ?? throw new ArgumentNullException(nameof(identifier));

            MarkPosition(target, identifier);
        }

        public override string ToString() {
            return Target + "." + Name;
        }
    }
}