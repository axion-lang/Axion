namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a character literal <see cref="Token" />.
    /// </summary>
    public class CharacterToken : Token, IClosingToken {
        public string RawValue { get; }

        public bool IsUnclosed { get; }

        public CharacterToken((int, int) startPosition, string value, string rawValue = null, bool isUnclosed = false)
            : base(TokenType.CharLiteral, startPosition, value) {
            if (rawValue == null) {
                rawValue = value;
            }
            RawValue   = rawValue;
            IsUnclosed = isUnclosed;
            RecomputeEndPosition();
        }

        private void RecomputeEndPosition() {
            EndColumn = StartColumn + RawValue.Length;
            EndColumn += IsUnclosed
                             ? 1
                             : 2; // quotes length
        }

        public override string ToAxionCode() {
            string result = Spec.CharLiteralQuote + RawValue;
            if (!IsUnclosed) {
                result += Spec.CharLiteralQuote;
            }
            return result + Whitespaces;
        }
    }
}