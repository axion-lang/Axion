namespace Axion.Tokens {
    /// <summary>
    ///     Represents a &lt;single-line comment&gt; <see cref="Token" />.
    /// </summary>
    public class SingleCommentToken : Token {
        public SingleCommentToken((int line, int column) location, string value)
            : base(TokenType.CommentLiteral, location, value) {
            // append length not included in value
            EndLnPos += Spec.CommentOnelineStart.Length;
        }
    }
}