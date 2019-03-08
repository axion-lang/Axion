using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        private SingleCommentToken ReadSingleLineComment() {
            stream.Move();

            // skip all until end of line or end of stream
            while (!stream.AtEndOfLine() && c != Spec.EndOfStream) {
                tokenValue.Append(c);
                stream.Move();
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
            if (unclosedMultilineComments.Count == 0) {
                // we're on '/*'
                stream.Move(Spec.MultiCommentStart.Length);
                unclosedMultilineComments.Add(
                    new MultilineCommentToken(tokenStartPosition, "", true)
                );
            }
            while (unclosedMultilineComments.Count > 0) {
                string nextPiece = c.ToString() + stream.Peek;
                // found comment end
                if (nextPiece == Spec.MultiCommentEnd) {
                    // don't add last comment '*/'
                    if (unclosedMultilineComments.Count != 1) {
                        tokenValue.Append(Spec.MultiCommentEnd);
                    }
                    stream.Move(2);
                    // decrease comment level
                    unclosedMultilineComments.RemoveAt(unclosedMultilineComments.Count - 1);
                }
                // found nested multiline comment start
                else if (nextPiece == Spec.MultiCommentStart) {
                    tokenValue.Append(Spec.MultiCommentStart);
                    stream.Move(2);
                    // increase comment level
                    unclosedMultilineComments.Add(
                        new MultilineCommentToken(tokenStartPosition, tokenValue.ToString(), true)
                    );
                }
                // went through end of stream
                else if (c == Spec.EndOfStream) {
                    unit.Blame(BlameType.UnclosedMultilineComment, tokenStartPosition, stream.Position);
                    return new MultilineCommentToken(
                        tokenStartPosition,
                        tokenValue.ToString(),
                        true
                    );
                }
                // found any other character
                else {
                    tokenValue.Append(c);
                    stream.Move();
                }
            }
            return new MultilineCommentToken(tokenStartPosition, tokenValue.ToString());
        }
    }
}