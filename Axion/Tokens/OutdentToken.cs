namespace Axion.Tokens {
    /// <summary>
    ///     Represents a &lt;indentation decrease&gt; <see cref="Token" />.
    /// </summary>
    public class OutdentToken : Token {
        public OutdentToken((int line, int column) location, string value)
            : base(TokenType.Indent, location, value) {
        }
    }
}