using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'comment' literal placed on multiple lines.
    /// </summary>
    public class CommentToken : Token {
        public bool IsSingleLine { get; }
        public bool IsUnclosed   { get; }

        public CommentToken(
            string   value,
            Position startPosition = default,
            bool     isSingleLine  = false,
            bool     isUnclosed    = false
        ) : base(TokenType.Comment, value, startPosition) {
            IsUnclosed   = isUnclosed;
            IsSingleLine = isSingleLine;

            // compute position
            int endCol;
            if (IsSingleLine) {
                endCol = Span.End.Column + CommentStart.Length;
            }
            else {
                string[] lines = Value.Split(
                    EndOfLines,
                    StringSplitOptions.None
                );
                int commentMarkLength = MultiCommentStart.Length;
                endCol = Span.End.Column;
                if (lines.Length == 1) {
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
            }

            Span = new Span(Span.Start, (Span.End.Line, endCol));
        }

        internal override void ToAxionCode(CodeBuilder c) {
            if (IsSingleLine) {
                c.Write(CommentStart + Value + EndWhitespaces);
            }

            else {
                c.Write(
                    IsUnclosed
                        ? MultiCommentStart + Value
                        // closed
                        : MultiCommentStart + Value + MultiCommentEnd
                );
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (IsSingleLine) {
                c.Write("//" + Value);
            }

            else {
                c.Write(
                    IsUnclosed
                        ? "/*" + Value
                        // closed
                        : "/*" + Value + "*/"
                );
            }
        }
    }
}