using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Tokens;

namespace Axion.Core.Visual {
    internal class ConsoleSyntaxHighlighter {
        private readonly ConsoleCodeEditor editor;

        public ConsoleSyntaxHighlighter(ConsoleCodeEditor codeEditor) {
            editor = codeEditor;
        }

        public void Highlight(IEnumerable<string> codeLines, ref List<SyntaxError> errors, ref List<SyntaxError> warnings) {
            var tokens = new LinkedList<Token>();
            var lexer = new Lexer(
                string.Join(Spec.EndOfLine.ToString(), codeLines),
                tokens,
                errors,
                warnings,
                SourceProcessingOptions.PreserveWhitespaces
            );
            lexer.Process();
            HighlightTokens(tokens);
        }

        private void HighlightTokens(LinkedList<Token> tokens) {
            int indentLength = 0;
            for (var i = 0; i < tokens.Count; i++) {
                Token token = tokens.ElementAt(i);
                editor.SetCursor(token.StartColumn, token.StartLine);

                if (token is OneLineCommentToken oneLineComment) {
                    ConsoleUI.Write((oneLineComment.ToAxionCode(), ConsoleColor.DarkGreen));
                }
                else if (token is MultilineCommentToken multilineComment) {
                    editor.WriteMultilineValue(multilineComment, ConsoleColor.DarkGreen);
                }
                else if (token is NumberToken number) {
                    ConsoleUI.Write((number.ToAxionCode(), ConsoleColor.Yellow));
                }
                else if (token is InterpolatedStringToken interpolatedString) {
                    // prefixes
                    if (interpolatedString.Options.HasFlag(StringLiteralOptions.Format)) {
                        ConsoleUI.Write(("f", ConsoleColor.Cyan));
                    }
                    if (interpolatedString.Options.HasFlag(StringLiteralOptions.Raw)) {
                        ConsoleUI.Write(("r", ConsoleColor.Cyan));
                    }

                    // opening quotes
                    int quotesCount = interpolatedString.Options.HasFlag(StringLiteralOptions.Multiline)
                                          ? 3
                                          : 1;
                    ConsoleUI.Write((new string(interpolatedString.Quote, quotesCount), ConsoleColor.DarkYellow));

                    // interpolations
                    var interpolationI = 0;
                    for (var index = 0; index < interpolatedString.Value.Length; index++) {
                        char c = interpolatedString.Value[index];
                        if (interpolationI < interpolatedString.Interpolations.Length &&
                            index == interpolatedString.Interpolations[interpolationI].StartIndex) {
                            Interpolation interpolation = interpolatedString.Interpolations[interpolationI];
                            Console.Write('{');
                            HighlightTokens(interpolation.Tokens);
                            Console.Write('}');
                            index += interpolation.Length - 1; // -1 because iteration incremented it.
                            interpolationI++;
                            continue;
                        }
                        if (c == Spec.EndOfLine) {
                            editor.MoveToNextLineStart();
                            continue;
                        }
                        ConsoleUI.Write((c.ToString(), ConsoleColor.DarkYellow));
                    }

                    // closing quotes
                    if (!interpolatedString.IsUnclosed) {
                        ConsoleUI.Write((new string(interpolatedString.Quote, quotesCount), ConsoleColor.DarkYellow));
                    }
                }
                else if (token is StringToken stringToken) {
                    editor.WriteMultilineValue(stringToken, ConsoleColor.DarkYellow);
                }
                else if (token is CharacterToken charToken) {
                    ConsoleUI.Write((charToken.ToAxionCode(), ConsoleColor.DarkYellow));
                }
                else if (token is OperatorToken operatorToken) {
                    ConsoleUI.Write((operatorToken.ToAxionCode(), ConsoleColor.Red));
                }
                else if (token is KeywordToken keyword) {
                    ConsoleUI.Write((keyword.ToAxionCode(), ConsoleColor.DarkCyan));
                }
                else if (token is IndentToken) {
                    indentLength += 4;
                }
                else if (token is OutdentToken) {
                    if (indentLength >= 4)
                    indentLength -= 4;
                }
                else if (token is EndOfStreamToken) {
                    break;
                }
                else if (token is EndOfLineToken) {
                    editor.MoveToNextLineStart();
                    Console.Write(new string(' ', indentLength));
                }
                else {
                    ConsoleUI.Write(token.Value);
                }
            }
        }
    }
}