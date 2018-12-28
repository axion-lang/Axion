using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        private SingleCommentToken ReadSingleComment() {
            Stream.Move();

            // skip all until end of line or end of file
            while (!Stream.AtEndOfLine() && c != Spec.EndOfStream) {
                tokenValue.Append(c);
                Stream.Move();
            }
            return new SingleCommentToken(tokenStartPosition, tokenValue.ToString());
        }

        /// <summary>
        ///     Gets a multiline comment from next piece of source.
        ///     Returns valid multiline comment token if next piece of source
        ///     if comment value is followed by multiline
        ///     comment closing sign.
        ///     Otherwise, returns unclosed multiline comment token.
        /// </summary>
        private MultilineCommentToken ReadMultilineComment() {
            if (_unclosedMultilineComments.Count == 0) {
                // we're on '/*'
                Stream.Move(Spec.MultiCommentStart.Length);
                _unclosedMultilineComments.Add(
                    new MultilineCommentToken(
                        tokenStartPosition,
                        "",
                        true
                    )
                );
            }
            while (_unclosedMultilineComments.Count > 0) {
                string nextPiece = c.ToString() + Stream.Peek;
                // found comment end
                if (nextPiece == Spec.MultiCommentEnd) {
                    // don't add last comment '*/'
                    if (_unclosedMultilineComments.Count != 1) {
                        tokenValue.Append(Spec.MultiCommentEnd);
                    }
                    Stream.Move(2);
                    // decrease comment level
                    _unclosedMultilineComments.RemoveAt(_unclosedMultilineComments.Count - 1);
                }
                // found nested multiline comment start
                else if (nextPiece == Spec.MultiCommentStart) {
                    tokenValue.Append(Spec.MultiCommentStart);
                    Stream.Move(2);
                    // increase comment level
                    _unclosedMultilineComments.Add(
                        new MultilineCommentToken(
                            tokenStartPosition,
                            tokenValue.ToString(),
                            true
                        )
                    );
                }
                // went through end of file
                else if (c == Spec.EndOfStream) {
                    Blame(BlameType.UnclosedMultilineComment, tokenStartPosition, Stream.Position);
                    return new MultilineCommentToken(
                        tokenStartPosition,
                        tokenValue.ToString(),
                        true
                    );
                }
                // found any other character
                else {
                    tokenValue.Append(c);
                    Stream.Move();
                }
            }
            return new MultilineCommentToken(tokenStartPosition, tokenValue.ToString());
        }
    }
}