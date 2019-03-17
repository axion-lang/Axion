using System;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'comment' literal placed on multiple lines.
    /// </summary>
    public class MultilineCommentToken : Token {
        public bool IsUnclosed { get; }

        public MultilineCommentToken(
            string   value,
            bool     isUnclosed    = false,
            Position startPosition = default
        )
            : base(TokenType.Comment, value, startPosition) {
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

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + (IsUnclosed
                ? Spec.MultiCommentStart + Value
                // closed
                : Spec.MultiCommentStart + Value + Spec.MultiCommentEnd) + Whitespaces;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + (IsUnclosed
                       ? "/*" + Value
                       // closed
                       : "/*" + Value + "*/") + Whitespaces;
        }
    }
}