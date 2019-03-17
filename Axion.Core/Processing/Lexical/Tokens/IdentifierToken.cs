namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'identifier'.
    /// </summary>
    public class IdentifierToken : Token {
        public IdentifierToken(string value, Position startPosition = default) :
            base(TokenType.Identifier, value, startPosition) { }
    }
}