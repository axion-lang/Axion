namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a character literal <see cref="Token" />.
    /// </summary>
    public class CharacterToken : Token, IClosingToken {
        internal readonly string UnescapedValue;

        public bool IsUnclosed { get; }

        public CharacterToken((int, int) startPosition, string value, string unescapedValue = null, bool isUnclosed = false)
            : base(TokenType.CharLiteral, startPosition, value) {
            if (unescapedValue == null) {
                unescapedValue = value;
            }
            UnescapedValue = unescapedValue;
            IsUnclosed     = isUnclosed;
            EndColumn += IsUnclosed
                             ? 1
                             : 2; // quotes length
        }

        public override string ToAxionCode() {
            string result = Spec.CharLiteralQuote + Value;
            if (!IsUnclosed) {
                result += Spec.CharLiteralQuote;
            }
            return result + Whitespaces;
        }
    }
}