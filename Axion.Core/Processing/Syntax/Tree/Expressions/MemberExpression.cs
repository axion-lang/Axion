using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class MemberExpression : Expression {
        [JsonProperty]
        internal Expression Target { get; }

        [JsonProperty]
        internal NameExpression Name { get; }

        public MemberExpression(Expression target, NameExpression name) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Name   = name ?? throw new ArgumentNullException(nameof(name));

            MarkPosition(target, name);
        }

        public override string ToString() {
            return base.ToString() + ":" + Name;
        }
    }
}