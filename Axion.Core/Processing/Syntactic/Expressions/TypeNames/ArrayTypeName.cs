using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         array-type:
    ///             type '[' ']';
    ///     </c>
    /// </summary>
    public class ArrayTypeName : TypeName {
        private TypeName elementType = null!;

        public TypeName ElementType {
            get => elementType;
            set => elementType = Bind(value);
        }

        public ArrayTypeName(Node parent) : base(parent) { }

        public ArrayTypeName Parse() {
            SetSpan(
                () => {
                    if (ElementType == null) {
                        ElementType = Parse(this);
                    }

                    Stream.Eat(OpenBracket);
                    Stream.Eat(CloseBracket);
                }
            );
            return this;
        }
    }
}
