namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;language keyword&gt; <see cref="Token" />.
    /// </summary>
    internal class KeywordToken : Token {
        public KeywordToken((int, int) startPosition, string value, TokenType type)
            : base(type, startPosition, value) {
        }
    }
}