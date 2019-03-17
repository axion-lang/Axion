using System;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class ArrayTypeName : TypeName {
        public readonly TypeName ElementType;

        public ArrayTypeName(TypeName elementType) {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public ArrayTypeName(TypeName elementType, Token end) : this(elementType) {
            MarkPosition(ElementType, end);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + ElementType + "[]";
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + ElementType + "[]";
        }
    }
}