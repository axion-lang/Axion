namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a &lt;single-line comment&gt; <see cref="Token" />.
    /// </summary>
    public class SingleCommentToken : Token {
        public SingleCommentToken((int, int) startPosition, string value)
            : base(TokenType.Comment, startPosition, value) {
            // append length that is not included in 'Value'
            EndColumn += Spec.SingleCommentStart.Length;
        }

        public override string ToAxionCode() {
            return Spec.SingleCommentStart + Value + Whitespaces;
        }
    }
}