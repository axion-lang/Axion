using System;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class VarDefinitionExpression : LeftRightExpression {
        private TypeName type;

        [JsonProperty]
        public TypeName Type {
            get => type;
            private set {
                value.Parent = this;
                type         = value;
            }
        }

        public VarDefinitionExpression(Expression left, TypeName type, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Type  = type ?? throw new ArgumentNullException(nameof(type));
            Right = right;
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + ": " + Type + (Right != null ? " = " + Right : "");
        }
    }
}