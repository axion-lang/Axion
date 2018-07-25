namespace Axion.Tokens {
    /// <summary>
    ///     Represents an &lt;indentation increase&gt; <see cref="Token" />.
    /// </summary>
    public class IndentToken : Token {
        public IndentToken((int line, int column) location, string value)
            : base(TokenType.Indent, location, value) {
        }
    }
}