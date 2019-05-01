using System;
using System.Collections.Generic;
using System.Drawing;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using ConsoleExtensions;
using static System.ConsoleColor;
using static Axion.Core.Specification.TokenType;

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
                bool tokenHighlightingNotNeeded =
                    !foundRenderStart
                    && (token.Span.EndPosition.Line < renderPosition.Y
                        || token.Span.EndPosition.Line == renderPosition.Y
                        && token.Span.EndPosition.Column <= renderPosition.X
                        || token.Is(End));
                // BUG (UI) if code has error before that token, and it's fixed with next char, it'll be highlighted improperly (e. g. type '0..10')

                if (tokenHighlightingNotNeeded) {
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

                if (token.Is(End)) {
                    break;
                }

                string text = token.ToOriginalAxionCode();

                if (token.Is(Whitespace)) {
                    values.Add(new ColoredValue(text, DarkGray));
                }
                else if (token.Is(Newline)) {
                    values.Add(
                        values.Count > 0
                            ? new ColoredValue(text, values[values.Count - 1].Color)
                            : new ColoredValue(text, White)
                    );
                }
                else if (token.Is(Identifier)) {
                    // highlight error types
                    values.Add(
                        token.Value.EndsWith("Error")
                            ? new ColoredValue(text, DarkMagenta)
                            : new ColoredValue(text, Cyan)
                    );
                }
                else if (token is StringToken strToken
                         && strToken.Interpolations.Count > 0) {
                    HighlightInterpolatedString(strToken, values);
                }
                else {
                    ConsoleColor tokenColor = GetSimpleTokenColor(token);
                    values.Add(new ColoredValue(text, tokenColor));
                }
            }
        }

        private static ConsoleColor GetSimpleTokenColor(Token token) {
            ConsoleColor tokenColor;
            if (token.Is(Comment)) {
                tokenColor = DarkGreen;
            }
            else if (token.Is(TokenType.String)) {
                tokenColor = DarkYellow;
            }
            else if (token.Is(Character)) {
                tokenColor = DarkYellow;
            }
            else if (token.Is(Number)) {
                tokenColor = Yellow;
            }
            else if (token is OperatorToken) {
                tokenColor = Red;
            }
            else if (token is SymbolToken) {
                tokenColor = DarkGray;
            }
            else if (Spec.Keywords.ContainsValue(token.Type)) {
                tokenColor = DarkCyan;
            }
            else {
                tokenColor = White;
            }

            return tokenColor;
        }

        private void HighlightInterpolatedString(
            StringToken        token,
            List<ColoredValue> values
        ) {
            // prefixes
            values.Add(new ColoredValue(token.Options.GetPrefixes(), Cyan));

            int quotesCount = token.Options.QuotesCount;
            // opening quotes
            values.Add(new ColoredValue(new string(token.Options.Quote, quotesCount), DarkYellow));

            // interpolations
            var interpolationI = 0;
            for (var i = 0; i < token.Value.Length; i++) {
                char c = token.Value[i];
                if (interpolationI < token.Interpolations.Count
                    && i == token.Interpolations[interpolationI].StartIndex) {
                    Interpolation interpolation = token.Interpolations[interpolationI];
                    values.Add(new ColoredValue("{", White));
                    HighlightTokens(interpolation.Tokens, values, true);
                    values.Add(new ColoredValue("}", White));
                    i += interpolation.Length - 1; // -1 because iteration incremented it.
                    interpolationI++;
                    continue;
                }

                values.Add(new ColoredValue(c.ToString(), DarkYellow));
            }

            // closing quotes
            if (!token.IsUnclosed) {
                values.Add(
                    new ColoredValue(
                        new string(token.Options.Quote, quotesCount),
                        DarkYellow
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
                        diffItem.Value += values[i].Value;
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