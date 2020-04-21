using Axion.Core.Processing.Lexical.Tokens;
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

        private Token? openingBracket;

        public Token? OpeningBracket {
            get => openingBracket;
            set => openingBracket = BindNullable(value);
        }

        private Token? closingBracket;

        public Token? ClosingBracket {
            get => closingBracket;
            set => closingBracket = BindNullable(value);
        }

        public ArrayTypeName(Node parent) : base(parent) { }

        public ArrayTypeName Parse() {
            ElementType ??= Parse(this);

            OpeningBracket = Stream.Eat(OpenBracket);
            ClosingBracket = Stream.Eat(CloseBracket);
            return this;
        }
    }
}
