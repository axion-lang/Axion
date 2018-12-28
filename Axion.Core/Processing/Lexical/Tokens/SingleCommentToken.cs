using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a &lt;single-line comment&gt; <see cref="Token" />.
    /// </summary>
    public class SingleCommentToken : Token {
        public SingleCommentToken(Position startPosition, string value)
            : base(TokenType.Comment, startPosition, value) {
            // append length that is not included in 'Value'
            int endCol = Span.End.Column + Spec.SingleCommentStart.Length;
            Span = new Span(Span.Start, (Span.End.Line, endCol));
        }

        public override string ToAxionCode() {
            return Spec.SingleCommentStart + Value + Whitespaces;
        }
    }
}