namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents an &lt;end of line&gt; ( \n or \r\n ) <see cref="Token" />.
    /// </summary>
    public class EndOfLineToken : Token {
        public EndOfLineToken((int line, int column) location)
            : base(TokenType.Newline, location, Spec.EndLine.ToString()) {
        }
    }
}