namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents an identifier <see cref="Token" />.
    /// </summary>
    public class IdentifierToken : Token {
        public IdentifierToken((int line, int column) location, string value)
            : base(TokenType.Identifier, location, value) {
        }
    }
}