using System;
using System.Collections.Generic;
using System.Drawing;
using Axion.Core.Processing;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using FluentConsole;

namespace Axion.Core.Visual {
    /// <summary>
    ///     Implementation of Axion language
    ///     syntax highlighter for console.
    /// </summary>
    internal class AxionSyntaxHighlighter : ISyntaxHighlighter {
        private Point renderPosition;

        /// <summary>
        ///     Creates a pairs of (text, color) from given source,
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
        /// <param name="blames"></param>
        public List<ColoredValue> Highlight(
            List<string>        codeLines,
            ref Point           lastRenderEndPosition,
            out List<Exception> blames
        ) {
            renderPosition = lastRenderEndPosition;
            var unit  = new SourceUnit(codeLines.ToArray());
            var lexer = new Lexer(unit);
            lexer.Process();
            blames = new List<Exception>(unit.Blames);

            var values = new List<ColoredValue>();

            HighlightTokens(unit.Tokens, values, false);
            MergeNeighbourColors(values);

            lastRenderEndPosition = renderPosition;
            return values;
        }

        private void HighlightTokens(
            List<Token>        tokens,
            List<ColoredValue> values,
            bool               foundRenderStart
        ) {
            foreach (Token token in tokens) {
                bool tokenShouldBeSkipped =
                    !foundRenderStart
                    && (token.Span.EndPosition.Line < renderPosition.Y
                        || token.Span.EndPosition.Line == renderPosition.Y
                        && token.Span.EndPosition.Column <= renderPosition.X
                        || token.Is(TokenType.End));
                // BUG (UI) if code has error before that token, and it's fixed with next char, it'll be highlighted improperly (e. g. type '0..10')

                #region Complex values

                if (tokenShouldBeSkipped) {
                    continue;
                }

                if (!foundRenderStart) {
                    // When we found token, closest to last render position
                    // we should re-render this token to prevent invalid highlighting.
                    renderPosition = new Point(
                        token.Span.StartPosition.Column,
                        token.Span.StartPosition.Line
                    );
                    foundRenderStart = true;
                }

                if (token.Is(TokenType.End)) {
                    break;
                }

                if (token.Is(TokenType.Whitespace)) {
                    values.Add(
                        new ColoredValue(
                            new string(' ', token.Value.Length),
                            ConsoleColor.DarkGray
                        )
                    );
                    continue;
                }

                if (token.Is(TokenType.Newline)) {
                    var code = new CodeBuilder(OutLang.Axion);
                    token.ToOriginalAxionCode(code);
                    if (values.Count > 0) {
                        values[values.Count - 1].AppendValue(code.ToString());
                    }
                    else {
                        values.Add(
                            new ColoredValue(
                                code.ToString(),
                                ConsoleColor.White
                            )
                        );
                    }

                    continue;
                }

                if (token.Is(TokenType.Identifier)) {
                    var code = new CodeBuilder(OutLang.Axion);
                    token.ToOriginalAxionCode(code);
                    // highlight error types
                    if (token.Value.EndsWith("Error")) {
                        values.Add(
                            new ColoredValue(
                                code.ToString(),
                                ConsoleColor.DarkMagenta
                            )
                        );
                    }
                    else {
                        values.Add(
                            new ColoredValue(
                                code.ToString(),
                                ConsoleColor.Cyan
                            )
                        );
                    }

                    continue;
                }

                if (token is StringToken strToken
                    && strToken.Interpolations.Count > 0) {
                    HighlightInterpolatedString(strToken, values);
                    continue;
                }

                #endregion

                // simple values
                ConsoleColor tokenColor  = GetSimpleTokenColor(token);
                var          codeBuilder = new CodeBuilder(OutLang.Axion);
                token.ToOriginalAxionCode(codeBuilder);
                values.Add(new ColoredValue(codeBuilder.ToString(), tokenColor));
            }
        }

        private static ConsoleColor GetSimpleTokenColor(Token token) {
            ConsoleColor tokenColor;
            if (token.Is(TokenType.Comment)) {
                tokenColor = ConsoleColor.DarkGreen;
            }
            else if (token.Is(TokenType.String)) {
                tokenColor = ConsoleColor.DarkYellow;
            }
            else if (token.Is(TokenType.Character)) {
                tokenColor = ConsoleColor.DarkYellow;
            }
            else if (token.Is(TokenType.Number)) {
                tokenColor = ConsoleColor.Yellow;
            }
            else if (token is OperatorToken) {
                tokenColor = ConsoleColor.Red;
            }
            else if (token is SymbolToken) {
                tokenColor = ConsoleColor.DarkGray;
            }
            else if (Spec.Keywords.ContainsValue(token.Type)) {
                tokenColor = ConsoleColor.DarkCyan;
            }
            else {
                tokenColor = ConsoleColor.White;
            }

            return tokenColor;
        }

        private void HighlightInterpolatedString(StringToken token, List<ColoredValue> values) {
            // prefixes
            values.Add(new ColoredValue(token.Options.GetPrefixes(), ConsoleColor.Cyan));

            int quotesCount = token.Options.QuotesCount;
            // opening quotes
            values.Add(
                new ColoredValue(
                    new string(token.Options.Quote, quotesCount),
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
                values.Add(
                    new ColoredValue(
                        new string(token.Options.Quote, quotesCount),
                        ConsoleColor.DarkYellow
                    )
                );
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