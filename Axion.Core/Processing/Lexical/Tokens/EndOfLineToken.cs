namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an &lt;end of line&gt; ( \n or \r\n ) <see cref="Token" />.
    /// </summary>
    public class EndOfLineToken : Token {
        public EndOfLineToken((int, int) startPosition, string tokenValue = "\n", string whitespaces = "")
            : base(TokenType.Newline, startPosition, tokenValue, whitespaces) {
        }
    }
}