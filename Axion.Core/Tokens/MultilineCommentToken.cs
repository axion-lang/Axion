namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;Multiline comment&gt; <see cref="Token" />.
    /// </summary>
    public class MultilineCommentToken : Token {
        public MultilineCommentToken((int line, int column) location, string value)
            : base(TokenType.CommentLiteral, location, value) {
            // append length not included in value
            EndColumnPos += Spec.CommentMultilineStart.Length;
        }

        public override string ToAxionCode() {
            return Spec.CommentMultilineStart +
                   Value +
                   Spec.CommentMultilineEnd;
        }
    }
}