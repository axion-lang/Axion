using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         array_type:
    ///             type '[' ']';
    ///     </c>
    /// </summary>
    public class ArrayTypeName : TypeName {
        private TypeName elementType;

        public TypeName ElementType {
            get => elementType;
            set => SetNode(ref elementType, value);
        }

        public ArrayTypeName(Expr parent = null, TypeName elementType = null) : base(parent) {
            ElementType = elementType;
        }

        public ArrayTypeName Parse() {
            SetSpan(
                () => {
                    if (ElementType == null) {
                        ElementType = ParseTypeName();
                    }

                    Stream.Eat(OpenBracket);
                    Stream.Eat(CloseBracket);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(ElementType, "[]");
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(ElementType, "[]");
        }

        public override void ToPython(CodeWriter c) {
            c.Write("List[", ElementType, "]");
        }
    }
}