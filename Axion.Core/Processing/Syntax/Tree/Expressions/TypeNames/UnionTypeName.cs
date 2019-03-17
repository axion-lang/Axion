using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class UnionTypeName : TypeName {
        public readonly TypeName Left;
        public readonly TypeName Right;

        public UnionTypeName(TypeName left, TypeName right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            MarkPosition(Left, Right);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Left + " | " + Right;
        }
    }
}