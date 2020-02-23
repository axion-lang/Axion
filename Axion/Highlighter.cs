using System;
using System.Collections.Generic;
using System.Drawing;
using Axion.Core;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Source;
using Axion.Core.Specification;
using CodeConsole;
using static System.ConsoleColor;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion {
    /// <summary>
    ///     Implementation of Axion language
    ///     syntax highlighter for console.
    /// </summary>
    internal class AxionSyntaxHighlighter : ISyntaxHighlighter {
        private Point renderPosition;

        public List<ColoredValue> Highlight(
            List<string>        codeLines,
            ref Point           lastRenderEndPosition,
            out List<Exception> blames
        ) {
            renderPosition = lastRenderEndPosition;
            SourceUnit src = SourceUnit.FromLines(codeLines.ToArray());
            Compiler.Lex(src);
            blames = new List<Exception>(src.Blames);
            var values = new List<ColoredValue>();
            HighlightTokens(src.TokenStream.Tokens, values, false);
            MergeNeighbourColors(values);
            lastRenderEndPosition = renderPosition;
            return values;
        }

        public List<ColoredValue> Highlight(string code) {
            SourceUnit src = SourceUnit.FromCode(code);
            Compiler.Lex(src);
            var values = new List<ColoredValue>();
            HighlightTokens(src.TokenStream.Tokens, values, false);
            MergeNeighbourColors(values);
            return values;
        }

        private void HighlightTokens(
            IEnumerable<Token> tokens,
            List<ColoredValue> values,
            bool               foundRenderStart
        ) {
            foreach (Token token in tokens) {
                bool tokenHighlightingNotNeeded =
                    !foundRenderStart
                 && (token.End.Line < renderPosition.Y
                  || token.End.Line   == renderPosition.Y
                  && token.End.Column <= renderPosition.X
                  && !token.Is(Newline)
                    );

                if (tokenHighlightingNotNeeded) {
                    continue;
                }

                if (!foundRenderStart) {
                    // When we found token, closest to last render position
                    // we should re-render this token to prevent invalid highlighting.
                    renderPosition = new Point(
                        token.Start.Column,
                        token.Start.Line
                    );
                    foundRenderStart = true;
                }

                if (token.Is(End)) {
                    break;
                }

                string text = token.Value + token.EndingWhite;
                if (token.Is(Whitespace)) {
                    values.Add(new ColoredValue(text, DarkGray));
                }
                else if (token.Is(Newline)) {
                    values.Add(
                        values.Count > 0
                            ? new ColoredValue(text, values[^1].Color)
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
            else if (token.Is(CustomKeyword)) {
                tokenColor = Magenta;
            }
            else if (token is OperatorToken) {
                tokenColor = Red;
            }
            else if (Spec.Keywords.ContainsValue(token.Type)) {
                tokenColor = DarkCyan;
            }
            else {
                tokenColor = White;
            }

            return tokenColor;
        }

        private static void MergeNeighbourColors(IList<ColoredValue> values) {
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