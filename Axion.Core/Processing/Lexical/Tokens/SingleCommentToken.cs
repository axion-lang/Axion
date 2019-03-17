using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'comment' literal placed on a single line.
    /// </summary>
    public class SingleCommentToken : Token {
        public SingleCommentToken(string value, Position startPosition = default) : base(
            TokenType.Comment,
            value,
            startPosition
        ) {
            // append length that is not included in 'Value'
            int endCol = Span.EndPosition.Column + Spec.SingleCommentStart.Length;
            Span = new Span(Span.StartPosition, (Span.EndPosition.Line, endCol));
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Spec.SingleCommentStart + Value + Whitespaces;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + "//" + Value + Whitespaces;
        }
    }
}