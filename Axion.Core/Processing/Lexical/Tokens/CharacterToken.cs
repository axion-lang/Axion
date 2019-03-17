using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'character' literal.
    /// </summary>
    public class CharacterToken : Token {
        public string RawValue   { get; }
        public bool   IsUnclosed { get; }

        public CharacterToken(
            string   value,
            string   rawValue      = null,
            bool     isUnclosed    = false,
            Position startPosition = default
        ) : base(TokenType.Character, value, startPosition) {
            RawValue   = rawValue ?? value;
            IsUnclosed = isUnclosed;
            RecomputeEndPosition();
        }

        private void RecomputeEndPosition() {
            int endCol = Span.StartPosition.Column
                         + RawValue.Length
                         + (IsUnclosed ? 1 : 2); // quotes length
            Span = new Span(Span.StartPosition, (Span.EndPosition.Line, endCol));
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + Spec.CharacterLiteralQuote.ToString() + RawValue;
            if (!IsUnclosed) {
                c += Spec.CharacterLiteralQuote.ToString();
            }

            return c + Whitespaces;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c = c + "'" + RawValue;
            if (!IsUnclosed) {
                c += "'";
            }

            return c + Whitespaces;
        }
    }
}