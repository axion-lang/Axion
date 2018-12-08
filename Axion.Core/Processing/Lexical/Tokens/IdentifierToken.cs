namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an identifier <see cref="Token" />.
    /// </summary>
    public class IdentifierToken : Token {
        public IdentifierToken((int, int) startPosition, string value)
            : base(TokenType.Identifier, startPosition, value) {
        }
    }
}