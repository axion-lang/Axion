using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class IndexExpression : Expression {
        private Expression target;

        [JsonProperty]
        internal Expression Target {
            get => target;
            set {
                value.Parent = this;
                target       = value;
            }
        }

        private Expression index;

        [JsonProperty]
        internal Expression Index {
            get => index;
            set {
                value.Parent = this;
                index        = value;
            }
        }

        public IndexExpression(Expression target, Expression index) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Index  = index ?? throw new ArgumentNullException(nameof(index));

            MarkStart(target);
            MarkEnd(index);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Target + "[" + Index + "]";
        }
    }
}