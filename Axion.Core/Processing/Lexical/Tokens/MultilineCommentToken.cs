using System;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a &lt;Multiline comment&gt; <see cref="Token" />.
    /// </summary>
    public class MultilineCommentToken : Token, IClosingToken {
        public bool IsUnclosed { get; }

        public MultilineCommentToken((int, int) startPosition, string value, bool isUnclosed = false)
            : base(TokenType.Comment, startPosition, value) {
            IsUnclosed = isUnclosed;

            int linesCount        = value.Split(Spec.EndOfLines, StringSplitOptions.None).Length;
            int commentMarkLength = Spec.MultiCommentStart.Length;
            if (linesCount == 1) {
                if (isUnclosed) {
                    EndColumn += commentMarkLength;
                }
                else {
                    EndColumn += commentMarkLength * 2;
                }
            }
            else if (!isUnclosed) {
                EndColumn += commentMarkLength;
            }
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