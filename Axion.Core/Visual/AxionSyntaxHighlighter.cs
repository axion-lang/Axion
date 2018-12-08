using System;
using System.Collections.Generic;
using System.Drawing;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
using ConsoleExtensions;

namespace Axion.Core.Visual {
    internal class AxionSyntaxHighlighter : ISyntaxHighlighter {
        public ConsoleCodeEditor Editor { get; set; }

        private Point renderPosition;

        /// <summary>
        ///     Creates a pairs (text, color) from given source,
        ///     After that, moves cursor to last render position
        ///     and cleans all code after cursor.
        ///     Next you just need to write given values to the editor.
        /// </summary>
        /// <param name="codeLines">
        ///     Given source.
        /// </param>
        /// <param name="lastRenderEndPosition">
        ///     Last position of cursor, where code was correctly rendered.
        /// </param>
        /// <param name="errors">
        ///     Errors, occurred during code analysis.
        /// </param>
        /// <param name="warnings">
        ///     Warnings, occurred during code analysis.
        /// </param>
        public List<ColoredValue> Highlight(
            IEnumerable<string> codeLines,
            out Point           lastRenderEndPosition,
            List<Exception>     errors,
            List<Exception>     warnings
        ) {
            renderPosition = lastRenderEndPosition;
            var stream = new CharStream(codeLines);
            var tokens = new List<Token>();
            var lexer = new Lexer(
                stream,
                tokens,
                errors,
                warnings
            );
            lexer.Process();

            var values = new List<ColoredValue>();

            HighlightTokens(tokens, values, false);
            MergeNeighbourColors(values);

            lastRenderEndPosition = renderPosition;
            return values;
        }

        private void HighlightTokens(
            List<Token>        tokens,
            List<ColoredValue> values,
            bool               foundRenderStart
        ) {
            for (var i = 0; i < tokens.Count; i++) {
                Token token = tokens[tokens.Count - 1];

                bool tokenShouldBeSkipped =
                    !foundRenderStart
                 && (token.EndLine < renderPosition.Y
                  || token.EndLine == renderPosition.Y
                  && token.EndColumn <= renderPosition.X
                  || token.Type == TokenType.EndOfStream);
                // BUG: if code has error before that token, and it's fixed with next char, it'll be highlighted improperly (e. g. type '0..10')

                #region Complex values

                if (tokenShouldBeSkipped) {
                    continue;
                }
                if (!foundRenderStart) {
                    // When we found token, closest to last render position
                    // we should re-render this token to prevent invalid highlighting.
                    renderPosition   = new Point(token.StartColumn, token.StartLine);
                    foundRenderStart = true;
                }

                if (token.Type == TokenType.Whitespace) {
                    values.Add(new ColoredValue(new string(' ', token.Whitespaces.Length), ConsoleColor.DarkGray));
                    continue;
                }
                if (token.Type == TokenType.Newline) {
                    if (values.Count > 0) {
                        values[values.Count - 1].AppendValue(token.ToAxionCode());
                    }
                    else {
                        values.Add(new ColoredValue(token.ToAxionCode(), ConsoleColor.White));
                    }
                    continue;
                }
                if (token.Type == TokenType.Identifier) {
                    // highlight error types
                    if (token.Value.EndsWith("Error")) {
                        values.Add(new ColoredValue(token.ToAxionCode(), ConsoleColor.DarkMagenta));
                    }
                    else {
                        values.Add(new ColoredValue(token.ToAxionCode(), ConsoleColor.Cyan));
                    }
                    continue;
                }
                if (token is InterpolatedStringToken interpolatedStringToken) {
                    HighlightInterpolatedString(interpolatedStringToken, values);
                    continue;
                }

                #endregion

                // simple values
                ConsoleColor tokenColor = GetSimpleTokenColor(token);
                values.Add(new ColoredValue(token.ToAxionCode(), tokenColor));
            }
        }

        private static ConsoleColor GetSimpleTokenColor(Token token) {
            ConsoleColor tokenColor;
            if (token.Type == TokenType.Comment) {
                tokenColor = ConsoleColor.DarkGreen;
            }
            else if (token.Type == TokenType.String) {
                tokenColor = ConsoleColor.DarkYellow;
            }
            else if (token.Type == TokenType.Character) {
                tokenColor = ConsoleColor.DarkYellow;
            }
            else if (token is NumberToken) {
                tokenColor = ConsoleColor.Yellow;
            }
            else if (token is OperatorToken) {
                tokenColor = ConsoleColor.Red;
            }
            else if (token is KeywordToken) {
                tokenColor = ConsoleColor.DarkCyan;
            }
            else {
                tokenColor = ConsoleColor.White;
            }
            return tokenColor;
        }

        private void HighlightInterpolatedString(
            InterpolatedStringToken token,
            List<ColoredValue>      values
        ) {
            // prefixes
            values.Add(
                new ColoredValue(
                    token.Options.GetPrefixes(),
                    ConsoleColor.Cyan
                )
            );

            int quotesCount = token.Options.QuotesCount;
            // opening quotes
            values.Add(
                new ColoredValue(
                    new string(
                        token.Options.Quote,
                        quotesCount
                    ),
                    ConsoleColor.DarkYellow
                )
            );

            // interpolations
            var interpolationI = 0;
            for (var i = 0; i < token.Value.Length; i++) {
                char c = token.Value[i];
                if (interpolationI < token.Interpolations.Count
                 && i == token.Interpolations[interpolationI].StartIndex) {
                    Interpolation interpolation = token.Interpolations[interpolationI];
                    values.Add(new ColoredValue("{", ConsoleColor.White));
                    HighlightTokens(interpolation.Tokens, values, true);
                    values.Add(new ColoredValue("}", ConsoleColor.White));
                    i += interpolation.Length - 1; // -1 because iteration incremented it.
                    interpolationI++;
                    continue;
                }

                values.Add(new ColoredValue(c.ToString(), ConsoleColor.DarkYellow));
            }

            // closing quotes
            if (!token.IsUnclosed) {
                values.Add(new ColoredValue(new string(token.Options.Quote, quotesCount), ConsoleColor.DarkYellow));
            }
        }

        private static void MergeNeighbourColors(List<ColoredValue> values) {
            if (values.Count > 1) {
                // preprocess list
                // (merge neighbour values with one color)
                ColoredValue diffItem = values[0];
                for (var i = 1; i < values.Count;) {
                    if (values[i].Color == diffItem.Color) {
                        diffItem.AppendValue(values[i].Value);
                        values.RemoveAt(i);
                    }
                    else {
                        diffItem = values[i];
                        i++;
                    }
                }
            }
        }
    }
}