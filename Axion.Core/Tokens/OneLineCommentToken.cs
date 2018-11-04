namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;single-line comment&gt; <see cref="Token" />.
    /// </summary>
    public class OneLineCommentToken : Token {
        public OneLineCommentToken((int, int) startPosition, string value)
            : base(TokenType.CommentLiteral, startPosition, value) {
            // append length not included in value
            EndColumn += Spec.CommentOneLineStart.Length;
        }

        public override string ToAxionCode() {
            return Spec.CommentOneLineStart + Value;
        }
    }
}