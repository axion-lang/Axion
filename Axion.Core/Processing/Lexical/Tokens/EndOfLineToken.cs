namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'newline' ('\n' or '\r\n') mark.
    /// </summary>
    public class EndOfLineToken : Token {
        public EndOfLineToken(
            Position startPosition,
            string   tokenValue  = "\n",
            string   whitespaces = ""
        ) : base(TokenType.Newline, startPosition, tokenValue) {
            AppendValue(whitespaces);
        }
    }
}