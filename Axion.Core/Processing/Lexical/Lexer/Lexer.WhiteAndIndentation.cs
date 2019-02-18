using System.Text.RegularExpressions;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        #region Indentation properties

        /// <summary>
        ///     Count of indentation sequences
        ///     in current block of code.
        /// </summary>
        private int indentLevel;

        /// <summary>
        ///     Last code line indentation length.
        /// </summary>
        private int lastIndentLength;

        /// <summary>
        ///     Size of one indentation in source.
        ///     Determined by first indentation.
        /// </summary>
        private int oneIndentSize;

        /// <summary>
        ///     Indentation character used in current code.
        /// </summary>
        private char indentChar;

        /// <summary>
        ///     Returns <see langword="true" /> if code use mixed indentation
        ///     (partly by spaces, partly by tabs).
        /// </summary>
        private bool inconsistentIndentation;

        #endregion

        private EndOfLineToken ReadNewline() {
            // skip all newline characters
            while (c == '\n' || c == '\r') {
                tokenValue.Append(c);
                Stream.Move();
            }
            if (tokens.Count > 0 && tokens[tokens.Count - 1].Type == TokenType.Newline) {
                tokens[tokens.Count - 1].AppendValue(tokenValue.ToString());
                return null;
            }
            var endOfLineToken = new EndOfLineToken(tokenStartPosition, tokenValue.ToString());
            if (Spec.IsSpaceOrTab(c)) {
                return endOfLineToken;
            }

            // if last newline doesn't starts
            // with whitespace - reset indentation to 0
            // add newline at first
            tokens.Add(endOfLineToken);
            tokenStartPosition = Stream.Position;
            // then add outdents
            lastIndentLength = 0;
            while (indentLevel > 0) {
                tokens.Add(new OutdentToken(tokenStartPosition));
                indentLevel--;
            }
            return null;
        }

        private Token ReadWhite() {
            while (Spec.IsSpaceOrTab(c)) {
                tokenValue.Append(c);
                Stream.Move();
            }

            bool   nextIsIndent;
            string restOfLine = Stream.GetRestOfLine();
            // BUG: Outdent dont adds, when we have empty string with spaces.
            nextIsIndent = _mismatchingPairs.Count == 0
                        && !(
                                // rest is one-line comment
                                restOfLine.StartsWith(Spec.SingleCommentStart)
                                // or rest is multiline comment
                             || restOfLine.StartsWith(Spec.MultiCommentStart)
                                // and comment goes through end of line
                             && Regex.Matches(restOfLine, Spec.MultiCommentStartPattern).Count
                              > Regex.Matches(restOfLine, Spec.MultiCommentEndPattern).Count);

            // if it is 1st token,
            // set default indentation level.
            // TODO use another approach with indentation (leadingWhitespaces property)
            if (tokens.Count == 0) {
                lastIndentLength = tokenValue.Length;
                return new Token(
                    TokenType.Whitespace,
                    tokenStartPosition,
                    "",
                    tokenValue.ToString()
                );
            }

            if (tokens[tokens.Count - 1].Type == TokenType.Newline) {
                if (nextIsIndent) {
                    // handle empty string with whitespaces, make newline
                    if (restOfLine.Trim().Length == 0) {
                        tokenValue.Append(restOfLine);
                        Stream.Move(restOfLine.Length);
                        tokens.Add(new EndOfLineToken(tokenStartPosition, tokenValue.ToString()));
                        tokenValue.Clear();
                        tokenStartPosition = Stream.Position;
                    }
                    return ReadIndentation();
                }
                tokens[tokens.Count - 1].AppendValue(tokenValue.ToString());
            }
            else {
                tokens[tokens.Count - 1].AppendWhitespace(tokenValue.ToString());
            }
            return null;
        }

        private Token ReadIndentation() {
            // set indent character if it is unknown
            if (indentChar == '\0') {
                indentChar = tokenValue[0];
            }

            // compute indentation length
            var newIndentLength = 0;
            for (var i = 0; i < tokenValue.Length; i++) {
                char space = tokenValue[i];
                // check for consistency
                if (!inconsistentIndentation && indentChar != space) {
                    inconsistentIndentation = true;
                }

                if (space == ' ') {
                    newIndentLength++;
                }
                else if (space == '\t') {
                    // tab size computation borrowed from Python compiler
                    newIndentLength += 8 - newIndentLength % 8;
                }
            }

            if (oneIndentSize == 0 && !inconsistentIndentation) {
                oneIndentSize = newIndentLength;
            }

            Token indentationToken;
            if (newIndentLength > lastIndentLength) {
                // indent increased
                indentationToken = new IndentToken(tokenStartPosition, tokenValue.ToString());
                indentLevel++;
            }
            else if (newIndentLength < lastIndentLength) {
                // whitespace
                if (tokens.Count > 0) {
                    // append it to last token
                    tokens[tokens.Count - 1].AppendWhitespace(tokenValue.ToString());
                }

                int temp = newIndentLength;
                while (temp < lastIndentLength) {
                    // indent decreased
                    tokens.Add(new OutdentToken(tokenStartPosition));
                    indentLevel--;
                    temp += oneIndentSize;
                }
                indentationToken = null;
            }
            else {
                // whitespace
                if (tokens.Count > 0) {
                    // append it to last token
                    tokens[tokens.Count - 1].AppendWhitespace(tokenValue.ToString());
                }
                return null;
            }
            lastIndentLength = newIndentLength;

            // warn user about inconsistency
            if (inconsistentIndentation
             && options.HasFlag(SourceProcessingOptions.CheckIndentationConsistency)) {
                Blame(BlameType.InconsistentIndentation, tokenStartPosition, Stream.Position);
                // ignore future warnings
                options &= ~SourceProcessingOptions.CheckIndentationConsistency;
            }
            return indentationToken;
        }
    }
}