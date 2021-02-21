using System;
using System.Collections.Generic;
using System.Drawing;
using Axion.Core;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Specification;
using CodeConsole;
using static System.ConsoleColor;
using static Axion.Specification.TokenType;

namespace Axion {
    /// <summary>
    ///     Axion's (almost) semantic syntax highlighter for ScriptBench.
    /// </summary>
    internal class AxionSyntaxHighlighter : IScriptBenchSyntaxHighlighter {
        private Point renderPosition;

        public List<ColoredValue> Highlight(
            IEnumerable<string>          codeLines,
            ref Point                    lastRenderEndPosition,
            out IReadOnlyList<Exception> blames
        ) {
            renderPosition = lastRenderEndPosition;
            var src = Unit.FromLines(codeLines);
            // NOTE: direct call to Compiler.Lex works faster than Compiler.Process function chain.
            Compiler.Lex(src, ProcessingOptions.Default);
            blames = src.Blames;
            var values = HighlightTokens(src.TokenStream);
            lastRenderEndPosition = renderPosition;
            return values;
        }

        public List<ColoredValue> Highlight(string code) {
            var src = Unit.FromCode(code);
            Compiler.Lex(src, ProcessingOptions.Default);
            return HighlightTokens(src.TokenStream);
        }

        private List<ColoredValue> HighlightTokens(IEnumerable<Token> tokens) {
            var values           = new List<ColoredValue>();
            var foundRenderStart = false;
            foreach (var token in tokens) {
                // check if token highlighting is not needed (already highlighted)
                if (!foundRenderStart
                 && (token.End.Line < renderPosition.Y
                  || token.End.Line   == renderPosition.Y
                  && token.End.Column <= renderPosition.X
                  && !token.Is(Newline))) {
                    continue;
                }

                if (!foundRenderStart) {
                    // when found token closest to last render position,
                    // re-render it to prevent invalid highlighting.
                    renderPosition = new Point(
                        token.Start.Column,
                        token.Start.Line
                    );
                    foundRenderStart = true;
                }

                if (token.Is(End)) {
                    break;
                }

                var          text = token.Value + token.EndingWhite;
                ConsoleColor newColor;
                var          isWhite = false;

                if (token.Is(
                    Newline,
                    Whitespace,
                    Indent,
                    Outdent
                )) {
                    newColor = DarkGray;
                    isWhite  = true;
                }
                else if (token.Is(Identifier)) {
                    // highlight error types
                    newColor = token.Value.EndsWith("Error")
                        ? DarkMagenta
                        : Cyan;
                }
                else {
                    newColor = GetSimpleTokenColor(token);
                }

                values.Add(new ColoredValue(text, newColor, isWhite));
            }

            return values;
        }

        private static ConsoleColor GetSimpleTokenColor(Token token) {
            ConsoleColor tokenColor;
            if (token is OperatorToken) {
                tokenColor = Red;
            }
            else if (token.Is(Comment)) {
                tokenColor = DarkGray;
            }
            else if (token.Is(Number)) {
                tokenColor = Yellow;
            }
            else if (Spec.Keywords.ContainsValue(token.Type)) {
                tokenColor = DarkCyan;
            }
            else if (Spec.Punctuation.ContainsValue(token.Type)) {
                tokenColor = Gray;
            }
            else if (token.Is(TokenType.String, Character)) {
                tokenColor = DarkYellow;
            }
            else if (token.Is(CustomKeyword)) {
                tokenColor = Magenta;
            }
            else {
                tokenColor = White;
            }

            return tokenColor;
        }
    }
}
