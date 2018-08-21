namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;language keyword&gt; <see cref="Token" />.
    /// </summary>
    internal class KeywordToken : Token {
        public KeywordToken((int line, int column) location, TokenType type, string value)
            : base(type, location, value) {
        }
    }
}