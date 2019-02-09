namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'indentation increasing' mark.
    /// </summary>
    public class IndentToken : Token {
        public IndentToken(Position startPosition, string value) : base(TokenType.Indent, startPosition, value) {
        }
    }
}