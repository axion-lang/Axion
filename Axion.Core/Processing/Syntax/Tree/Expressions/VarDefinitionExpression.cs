using System;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class VarDefinitionExpression : Expression {
        public readonly TypeName Type;

        private Expression left;

        private Expression right;

        public VarDefinitionExpression(Expression left, TypeName type, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right;
            Type  = type ?? throw new ArgumentNullException(nameof(type));
        }

        [JsonProperty]
        internal Expression Left {
            get => left;
            set {
                value.Parent = this;
                left         = value;
            }
        }

        [JsonProperty]
        internal Expression Right {
            get => right;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                right = value;
            }
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + ": " + Type + (Right != null ? " = " + Right : "");
        }
    }
}