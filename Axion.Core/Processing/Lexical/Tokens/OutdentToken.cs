namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a &lt;indentation decrease&gt; <see cref="Token" />.
    /// </summary>
    public class OutdentToken : Token {
        public OutdentToken(Position startPosition)
            : base(TokenType.Outdent, startPosition) {
        }
    }
}