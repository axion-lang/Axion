using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        private CommentToken ReadSingleLineComment() {
            // comment start mark
            Move();
            // skip all until end of line or end of stream
            while (!AtEndOfLine && c != Spec.EndOfCode) {
                tokenValue.Append(c);
                Move();
            }

            return new CommentToken(tokenValue.ToString(), tokenStartPosition, true);
        }

        /// <summary>
        ///     Gets a multiline comment from next piece of source.
        ///     Returns valid multiline comment token if next piece of source
        ///     if comment value is followed by multiline
        ///     comment closing sign.
        ///     Otherwise, returns unclosed multiline comment token.
        /// </summary>
        private CommentToken ReadMultilineComment() {
            if (unclosedMultilineComments.Count == 0) {
                // we're on '/*'
                Move(Spec.MultiCommentStart.Length);
                unclosedMultilineComments.Push(
                    new CommentToken("", tokenStartPosition, isUnclosed: true)
                );
            }

            while (unclosedMultilineComments.Count > 0) {
                string nextPiece = c.ToString() + Peek;
                // found comment end
                if (nextPiece == Spec.MultiCommentEnd) {
                    // don't add last comment '*/'
                    if (unclosedMultilineComments.Count != 1) {
                        tokenValue.Append(Spec.MultiCommentEnd);
                    }

                    Move(Spec.MultiCommentEnd.Length);

                    // decrease comment level
                    unclosedMultilineComments.Pop();
                }
                // found nested multiline comment start
                else if (nextPiece == Spec.MultiCommentStart) {
                    tokenValue.Append(Spec.MultiCommentStart);
                    Move(Spec.MultiCommentStart.Length);

                    // increase comment level
                    unclosedMultilineComments.Push(
                        new CommentToken(
                            tokenValue.ToString(),
                            tokenStartPosition,
                            isUnclosed: true
                        )
                    );
                }
                // went through end of stream
                else if (c == Spec.EndOfCode) {
                    unit.Blame(
                        BlameType.UnclosedMultilineComment,
                        tokenStartPosition,
                        Position
                    );
                    return new CommentToken(
                        tokenValue.ToString(),
                        tokenStartPosition,
                        isUnclosed: true
                    );
                }
                // found any other character
                else {
                    tokenValue.Append(c);
                    Move();
                }
            }

            return new CommentToken(tokenValue.ToString(), tokenStartPosition);
        }
    }
}