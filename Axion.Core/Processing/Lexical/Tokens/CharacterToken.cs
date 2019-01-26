using Axion.Core.Processing.Lexical.Tokens.Interfaces;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a character literal <see cref="Token" />.
    /// </summary>
    public class CharacterToken : Token, IClosingToken, ILiteralToken {
        public string RawValue { get; }

        public bool IsUnclosed { get; }

        public CharacterToken(Position startPosition, string value, string rawValue = null, bool isUnclosed = false)
            : base(TokenType.Character, startPosition, value) {
            if (rawValue == null) {
                rawValue = value;
            }
            RawValue   = rawValue;
            IsUnclosed = isUnclosed;
            RecomputeEndPosition();
        }

        private void RecomputeEndPosition() {
            int endCol = Span.StartPosition.Column + RawValue.Length;
            endCol += IsUnclosed
                          ? 1
                          : 2; // quotes length
            Span = new Span(Span.StartPosition, (Span.EndPosition.Line, endCol));
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