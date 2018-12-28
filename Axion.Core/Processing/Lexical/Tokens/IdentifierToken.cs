namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an identifier <see cref="Token" />.
    /// </summary>
    public class IdentifierToken : Token {
        public IdentifierToken(Position startPosition, string value, string whitespaces = "")
            : base(TokenType.Identifier, startPosition, value, whitespaces) {
        }
    }
}