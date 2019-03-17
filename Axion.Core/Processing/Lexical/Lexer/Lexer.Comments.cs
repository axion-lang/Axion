using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        private SingleCommentToken ReadSingleLineComment() {
            stream.Move();

            // skip all until end of line or end of stream
            while (!stream.AtEndOfLine()
                   && c != EndOfStream) {
                tokenValue.Append(c);
                stream.Move();
            }

            return new SingleCommentToken(tokenValue.ToString(), tokenStartPosition);
        }

        /// <summary>
        ///     Gets a multiline comment from next piece of source.
        ///     Returns valid multiline comment token if next piece of source
        ///     if comment value is followed by multiline
        ///     comment closing sign.
        ///     Otherwise, returns unclosed multiline comment token.
        /// </summary>
        private MultilineCommentToken ReadMultilineComment() {
            if (unclosedMultilineComments.Count == 0) {
                // we're on '/*'
                stream.Move(MultiCommentStart.Length);
                unclosedMultilineComments.Add(
                    new MultilineCommentToken("", true, tokenStartPosition)
                );
            }

            while (unclosedMultilineComments.Count > 0) {
                string nextPiece = c.ToString() + stream.Peek;
                // found comment end
                if (nextPiece == MultiCommentEnd) {
                    // don't add last comment '*/'
                    if (unclosedMultilineComments.Count != 1) {
                        tokenValue.Append(MultiCommentEnd);
                    }

                    stream.Move(MultiCommentEnd.Length);

                    // decrease comment level
                    unclosedMultilineComments.RemoveAt(unclosedMultilineComments.Count - 1);
                }
                // found nested multiline comment start
                else if (nextPiece == MultiCommentStart) {
                    tokenValue.Append(MultiCommentStart);
                    stream.Move(MultiCommentStart.Length);

                    // increase comment level
                    unclosedMultilineComments.Add(
                        new MultilineCommentToken(
                            tokenValue.ToString(),
                            true,
                            tokenStartPosition
                        )
                    );
                }
                // went through end of stream
                else if (c == EndOfStream) {
                    unit.Blame(
                        BlameType.UnclosedMultilineComment,
                        tokenStartPosition,
                        stream.Position
                    );
                    return new MultilineCommentToken(
                        tokenValue.ToString(),
                        true,
                        tokenStartPosition
                    );
                }
                // found any other character
                else {
                    tokenValue.Append(c);
                    stream.Move();
                }
            }

            return new MultilineCommentToken(tokenValue.ToString(), false, tokenStartPosition);
        }
    }
}