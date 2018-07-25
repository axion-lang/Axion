namespace Axion.Tokens {
    /// <summary>
    ///     Represents a &lt;Multiline comment&gt; <see cref="Token" />.
    /// </summary>
    public class MultipleCommentToken : Token {
        public MultipleCommentToken((int line, int column) location, string value)
            : base(TokenType.CommentLiteral, location, value) {
            // append length not included in value
            EndClPos += Spec.CommentMultilineStart.Length;
        }
    }
}