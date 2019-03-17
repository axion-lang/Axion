namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'newline' ('\n' or '\r\n') mark.
    /// </summary>
    public class EndOfLineToken : Token {
        public EndOfLineToken(string tokenValue = "\n", Position startPosition = default) : base(
            TokenType.Newline,
            tokenValue,
            startPosition
        ) { }
    }
}