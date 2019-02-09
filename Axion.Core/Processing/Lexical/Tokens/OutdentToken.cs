namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'indentation decreasing' mark.
    /// </summary>
    public class OutdentToken : Token {
        public OutdentToken(Position startPosition) : base(TokenType.Outdent, startPosition) {
        }
    }
}