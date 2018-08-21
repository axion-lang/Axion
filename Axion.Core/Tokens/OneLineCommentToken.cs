namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;single-line comment&gt; <see cref="Token" />.
    /// </summary>
    public class OneLineCommentToken : Token {
        public OneLineCommentToken((int line, int column) location, string value)
            : base(TokenType.CommentLiteral, location, value) {
            // append length not included in value
            EndLinePos += Spec.CommentOnelineStart.Length;
        }

        public override string ToAxionCode() {
            return Spec.CommentOnelineStart + Value;
        }
    }
}