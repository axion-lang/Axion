using System.Text.RegularExpressions;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;
using static Axion.Core.Specification.Spec;

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
                stream.Move();
            }

            if (tokens.Count > 0 && tokens[tokens.Count - 1].Is(Newline)) {
                tokens[tokens.Count - 1].AppendValue(tokenValue.ToString());
                return null;
            }

            var endOfLine = new EndOfLineToken(tokenValue.ToString(), tokenStartPosition);
            if (IsSpaceOrTab(c)) {
                return endOfLine;
            }

            // if last newline doesn't followed
            // by whitespace - reset indentation to 0.
            // add newline at first
            tokens.Add(endOfLine);
            tokenStartPosition = stream.Position;
            // then add outdents
            lastIndentLength = 0;
            while (indentLevel > 0) {
                tokens.Add(new OutdentToken(tokenStartPosition));
                indentLevel--;
            }

            return null;
        }

        private Token ReadWhite() {
            while (IsSpaceOrTab(c)) {
                tokenValue.Append(c);
                stream.Move();
            }

            // if it is 1st token,
            // set default indentation level.
            // TODO use another approach with indentation (leadingWhitespaces property)
            if (tokens.Count == 0) {
                lastIndentLength = tokenValue.Length;
                return new Token(
                    Whitespace,
                    tokenValue.ToString(),
                    tokenStartPosition
                );
            }

            string restOfLine = stream.GetRestOfLine();
            bool   inBraces   = mismatchingPairs.Count > 0;
            bool prevIsBinOp = tokens[tokens.Count - 1] is OperatorToken op
                               && op.Properties.InputSide == InputSide.Both;
            bool nextCanBeIndent =
                !inBraces
                && !prevIsBinOp
                && !(
                    // line is empty
                    restOfLine.Trim().Length == 0
                    // next is one-line comment
                    || restOfLine.StartsWith(SingleCommentStart)
                    // or multiline comment
                    || restOfLine.StartsWith(MultiCommentStart)
                    // that continues on the next line
                    && Regex.Matches(restOfLine, MultiCommentStartPattern).Count
                    > Regex.Matches(restOfLine, MultiCommentEndPattern).Count
                );

            // BUG: Outdent dont adds, when we have empty string with spaces.
            if (tokens[tokens.Count - 1].Is(Newline)) {
                if (nextCanBeIndent) {
                    // handle empty string with whitespaces, make newline
                    if (restOfLine.Trim().Length == 0) {
                        tokenValue.Append(restOfLine);
                        stream.Move(restOfLine.Length);
                        tokens.Add(new EndOfLineToken(tokenValue.ToString(), tokenStartPosition));
                        tokenValue.Clear();
                        tokenStartPosition = stream.Position;
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
                if (!inconsistentIndentation
                    && indentChar != space) {
                    inconsistentIndentation = true;
                }

                if (space == ' ') {
                    newIndentLength++;
                }
                else if (space == '\t') {
                    newIndentLength += 8 - newIndentLength % 8;
                }
            }

            if (oneIndentSize == 0
                && !inconsistentIndentation) {
                oneIndentSize = newIndentLength;
            }

            Token indentationToken;
            if (newIndentLength > lastIndentLength) {
                // indent increased
                indentationToken = new IndentToken(tokenValue.ToString(), tokenStartPosition);
                indentLevel++;
            }
            else {
                // whitespace
                if (tokens.Count > 0) {
                    tokens[tokens.Count - 1].AppendWhitespace(tokenValue.ToString());
                }

                if (newIndentLength < lastIndentLength) {
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
                    return null;
                }
            }

            lastIndentLength = newIndentLength;

            // warn user about inconsistency
            if (inconsistentIndentation
                && unit.Options.HasFlag(SourceProcessingOptions.CheckIndentationConsistency)) {
                unit.Blame(
                    BlameType.InconsistentIndentation,
                    tokenStartPosition,
                    stream.Position
                );
            }

            return indentationToken;
        }
    }
}