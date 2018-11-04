namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;Multiline comment&gt; <see cref="Token" />.
    /// </summary>
    public class MultilineCommentToken : Token {
        internal readonly bool IsUnclosed;

        public MultilineCommentToken((int, int) startPosition, string value, bool isUnclosed = false)
            : base(TokenType.CommentLiteral, startPosition, value) {
            IsUnclosed = isUnclosed;
            // append length not included in value
            EndColumn += Spec.CommentMultilineStart.Length;
        }

        public override string ToAxionCode() {
            return IsUnclosed
                       ? Spec.CommentMultilineStart +
                         Value
                       // closed
                       : Spec.CommentMultilineStart +
                         Value +
                         Spec.CommentMultilineEnd;
        }
    }
}