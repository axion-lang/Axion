namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents an &lt;end of input stream&gt; <see cref="Token" />.
    /// </summary>
    public class EndOfStreamToken : Token {
        public EndOfStreamToken((int, int) startPosition)
            : base(TokenType.EndOfStream, startPosition, Spec.EndOfStream.ToString()) {
        }
    }
}