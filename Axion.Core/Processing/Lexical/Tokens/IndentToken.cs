namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an &lt;indentation increase&gt; <see cref="Token" />.
    /// </summary>
    public class IndentToken : Token {
        public IndentToken((int, int) startPosition, string value)
            : base(TokenType.Indent, startPosition, value) {
        }
    }
}