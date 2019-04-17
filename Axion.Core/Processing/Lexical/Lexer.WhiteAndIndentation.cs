using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Lexical {
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

        #endregion

        private NewlineToken? ReadNewline() {
            // skip all newline characters
            while (c == '\n' || c == '\r') {
                tokenValue.Append(c);
                Move();
            }

            if (tokens.Count > 0 && tokens.Last().Is(Newline)) {
                tokens[tokens.Count - 1].AppendValue(tokenValue.ToString());
                return null;
            }

            var endOfLine = new NewlineToken(tokenValue.ToString(), tokenStartPosition);
            if (c.IsSpaceOrTab()) {
                return endOfLine;
            }

            // if last newline doesn't followed
            // by whitespace - reset indentation to 0.
            // add newline at first
            tokens.Add(endOfLine);
            tokenStartPosition = Position;
            // then add outdents
            lastIndentLength = 0;
            while (indentLevel > 0) {
                tokens.Add(new Token(Outdent, tokenStartPosition));
                indentLevel--;
            }

            return null;
        }

        private Token? ReadWhite() {
            while (c.IsSpaceOrTab()) {
                tokenValue.Append(c);
                Move();
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

            string restOfLine = GetRestOfLine();
            int    restLen    = restOfLine.Trim().Length;

            bool prevIsBinOp =
                tokens[tokens.Count - 1] is OperatorToken op
                && op.Properties.InputSide == InputSide.Both;

            bool hasUnclosedComment =
                restOfLine.StartsWith(Spec.MultiCommentStart)
                // that continues on the next line
                && Regex.Matches(restOfLine, Spec.MultiCommentStartPattern).Count
                > Regex.Matches(restOfLine, Spec.MultiCommentEndPattern).Count;

            // todo optimize
            var any = false;
            foreach (KeyValuePair<string, OperatorProperties> kvp in Spec.Operators) {
                if (kvp.Value.InputSide == InputSide.Both
                    && restOfLine.StartsWith(kvp.Key)) {
                    any = true;
                    break;
                }
            }

            bool nextCanBeIndent =
                !prevIsBinOp
                && !hasUnclosedComment
                && mismatchingPairs.Count == 0
                && restLen > 0
                && !restOfLine.StartsWith(Spec.CommentStart)
                && !any;

            if (tokens[tokens.Count - 1].Is(Newline)) {
                // handle empty string with whitespaces, make newline
                if (restLen == 0 && !Spec.EndOfLines.Contains(restOfLine)) {
                    tokenValue.Append(restOfLine);
                    Move(restOfLine.Length);
                    tokens.Add(new NewlineToken(tokenValue.ToString(), tokenStartPosition));
                    tokenValue.Clear();
                    tokenStartPosition = Position;
                    return ReadIndentation();
                }

                if (nextCanBeIndent) {
                    return ReadIndentation();
                }

                tokens[tokens.Count - 1].AppendValue(tokenValue.ToString());
            }
            else {
                tokens[tokens.Count - 1].AppendWhitespace(tokenValue.ToString());
            }

            return null;
        }

        private Token? ReadIndentation() {
            // set indent character if it is unknown
            if (indentChar == '\0') {
                indentChar = tokenValue[0];
            }

            var      consistent        = true;
            Position inconsistentStart = default;

            // compute indentation length
            var newIndentLength = 0;
            for (var i = 0; i < tokenValue.Length; i++) {
                char ch = tokenValue[i];
                // check for consistency
                if (consistent && indentChar != ch) {
                    inconsistentStart = (Position.Line, i);
                    consistent        = false;
                }

                if (ch == ' ') {
                    newIndentLength++;
                }
                else if (ch == '\t') {
                    if (oneIndentSize != 0) {
                        newIndentLength += oneIndentSize;
                    }
                    else {
                        newIndentLength += 8 - newIndentLength % 8;
                    }
                }
            }

            if (consistent && oneIndentSize == 0) {
                oneIndentSize = newIndentLength;
            }

            Token? indentationToken;
            if (newIndentLength > lastIndentLength) {
                // indent increased
                indentationToken = new Token(
                    Indent,
                    tokenValue.ToString(),
                    tokenStartPosition
                );
                indentLevel++;
            }
            else {
                // add whitespace to last newline
                if (tokens.Count > 0) {
                    tokens[tokens.Count - 1].AppendWhitespace(tokenValue.ToString());
                }

                while (newIndentLength < lastIndentLength) {
                    // indent decreased
                    tokens.Add(new Token(Outdent, tokenStartPosition));
                    indentLevel--;
                    lastIndentLength -= oneIndentSize;
                }

                indentationToken = null;
            }

            lastIndentLength = newIndentLength;

            // warn about inconsistency
            if (!consistent
                && unit.Options.HasFlag(SourceProcessingOptions.CheckIndentationConsistency)) {
                unit.Blame(
                    BlameType.InconsistentIndentation,
                    inconsistentStart,
                    Position
                );
            }

            return indentationToken;
        }
    }
}