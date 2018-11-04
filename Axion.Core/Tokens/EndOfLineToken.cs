namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents an &lt;end of line&gt; ( \n or \r\n ) <see cref="Token" />.
    /// </summary>
    public class EndOfLineToken : Token {
        public EndOfLineToken((int, int) startPosition)
            : base(TokenType.Newline, startPosition, Spec.EndOfLine.ToString()) {
        }
    }
}