namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an &lt;end of input stream&gt; <see cref="Token" />.
    /// </summary>
    public class EndOfStreamToken : Token {
        public EndOfStreamToken(Position startPosition)
            : base(TokenType.EndOfStream, startPosition) {
        }
    }
}