using System;
using Axion.Core.Processing.Lexical.Tokens.Interfaces;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a &lt;Multiline comment&gt; <see cref="Token" />.
    /// </summary>
    public class MultilineCommentToken : Token, IClosingToken {
        public bool IsUnclosed { get; }

        public MultilineCommentToken(Position startPosition, string value, bool isUnclosed = false)
            : base(TokenType.Comment, startPosition, value) {
            IsUnclosed = isUnclosed;

            int linesCount        = value.Split(Spec.EndOfLines, StringSplitOptions.None).Length;
            int commentMarkLength = Spec.MultiCommentStart.Length;
            int endCol            = Span.EndPosition.Column;
            if (linesCount == 1) {
                if (isUnclosed) {
                    endCol += commentMarkLength;
                }
                else {
                    endCol += commentMarkLength * 2;
                }
            }
            else if (!isUnclosed) {
                endCol += commentMarkLength;
            }
            Span = new Span(Span.StartPosition, (Span.EndPosition.Line, endCol));
        }

        public override string ToAxionCode() {
            return IsUnclosed
                       ? Spec.MultiCommentStart +
                         Value
                       // closed
                       : Spec.MultiCommentStart +
                         Value +
                         Spec.MultiCommentEnd + Whitespaces;
        }
    }
}