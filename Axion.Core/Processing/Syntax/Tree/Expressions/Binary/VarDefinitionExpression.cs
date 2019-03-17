using System;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class VarDefinitionExpression : LeftRightExpression {
        private TypeName type;

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

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + Left + ": " + Type;
            if (Right != null) {
                c = c + " = " + Right;
            }
            return c;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c = c + Type + " " + Left;
            if (Right != null) {
                c = c + " = " + Right;
            }
            return c;
        }
    }
}