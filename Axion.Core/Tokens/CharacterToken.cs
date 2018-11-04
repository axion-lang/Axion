namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a character literal <see cref="Token" />.
    /// </summary>
    public class CharacterToken : Token {
        internal readonly bool IsUnclosed;

        public CharacterToken((int, int) startPosition, string value, bool isUnclosed = false)
            : base(TokenType.CharLiteral, startPosition, value) {
            IsUnclosed = isUnclosed;
            EndColumn += IsUnclosed
                ? 1
                : 2; // quotes length
        }

        public override string ToAxionCode() {
            string result = Spec.CharLiteralQuote + Value;
            if (!IsUnclosed) {
                result += Spec.CharLiteralQuote;
            }
            return result;
        }
    }
}