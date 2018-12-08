using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        private Token ReadString(bool continueUnclosedString) {
            bool isStringEmpty = ReadStringPrefixes(
                ref continueUnclosedString,
                out StringLiteralOptions strOptions,
                out string delimiter
            );

            if (isStringEmpty) {
                return null;
            }

            var rawValue       = new StringBuilder();
            var interpolations = new List<Interpolation>();
            int startIndex     = Stream.CharIdx;

            StringToken result;

            void AddPiece(string piece) {
                tokenValue.Append(piece);
                rawValue.Append(piece);
            }

            while (true) {
                if (c == '\\') {
                    Stream.Move();
                    if (strOptions.IsRaw) {
                        AddPiece("\\");
                        continue;
                    }
                    (string value, string raw) value = ReadEscapeSequence();
                    tokenValue.Append(value.value);
                    rawValue.Append(value.raw);
                    continue;
                }

                // found end of line
                if (c == '\r' && strOptions.IsLineEndsNormalized) {
                    // normalize line endings
                    if (c == '\n') {
                        Stream.Move();
                    }
                    AddPiece("\n");
                }

                // check for end of line/file
                if (c == Spec.EndOfStream
                 || Stream.AtEndOfLine() && delimiter.Length == 1) {
                    result = new StringToken(
                        tokenStartPosition,
                        strOptions,
                        tokenValue.ToString(),
                        rawValue.ToString(),
                        true
                    );
                    _unclosedStrings.Add(result);
                    ReportError(ErrorType.UnclosedString, tokenStartPosition, Stream.Position);
                    return result;
                }

                // got non-escaped quote
                if (c == strOptions.Quote) {
                    (int line, int column) startPosition = Stream.Position;
                    Stream.Move();
                    // ending "
                    if (delimiter.Length == 1) {
                        strOptions.TrailingQuotes = "";
                        break;
                    }
                    if (c == strOptions.Quote) {
                        Stream.Move();
                        if (c == strOptions.Quote) {
                            // ending """
                            strOptions.TrailingQuotes = "";
                            Stream.Move();
                            break;
                        }
                        strOptions.TrailingQuotes += strOptions.Quote;
                    }
                    strOptions.TrailingQuotes += strOptions.Quote;

                    // unescaped quote in string - error
                    result = new StringToken(
                        tokenStartPosition,
                        strOptions,
                        tokenValue.ToString(),
                        rawValue.ToString(),
                        true
                    );
                    ReportError(ErrorType.UnescapedQuoteInStringLiteral, startPosition, Stream.Position);
                    return result;
                }

                // found string format sign
                if (c == '{' && strOptions.IsFormatted) {
                    ReadStringInterpolation(interpolations, startIndex, rawValue);
                    continue;
                }
                // else
                AddPiece(c.ToString());
                Stream.Move();
            }
            if (strOptions.IsFormatted) {
                if (interpolations.Count == 0) {
                    ReportWarning(
                        WarningType.RedundantStringFormatPrefix,
                        tokenStartPosition,
                        Stream.Position
                    );
                }
                return new InterpolatedStringToken(
                    tokenStartPosition,
                    strOptions,
                    interpolations,
                    tokenValue.ToString(),
                    rawValue.ToString()
                );
            }
            result = new StringToken(
                tokenStartPosition,
                strOptions,
                tokenValue.ToString(),
                rawValue.ToString()
            );
            return result;
        }

        private bool ReadStringPrefixes(
            ref bool                 continueUnclosedString,
            out StringLiteralOptions strOptions,
            out string               delimiter
        ) {
            if (continueUnclosedString) {
                StringToken stringToken = _unclosedStrings.Last();
                _unclosedStrings.RemoveAt(_unclosedStrings.Count - 1);
                strOptions = stringToken.Options;
                delimiter = new string(
                    stringToken.Options.Quote,
                    strOptions.QuotesCount
                );
            }
            else {
                strOptions = new StringLiteralOptions();
                {
                    // get letters before the string, process prefixes
                    (int line, int column) tempPosition = tokenStartPosition;
                    int                    tempCharIdx  = Stream.CharIdx;
                    // read string prefixes
                    var validPrefix = true;
                    while (tempPosition.column > 0 && validPrefix) {
                        tempCharIdx--;
                        tempPosition.column--;
                        char pfc = Stream.Source[tempCharIdx];
                        strOptions.AppendPrefix(
                            pfc,
                            out validPrefix,
                            out bool duplicatedPrefix
                        );
                        if (validPrefix) {
                            if (duplicatedPrefix) {
                                ReportWarning(WarningType.DuplicatedStringPrefix, tempPosition, Stream.Position);
                            }
                        }
                        // got invalid prefix letter or
                        // digit right before quote - error!
                        else if (char.IsLetterOrDigit(pfc)) {
                            ReportError(ErrorType.InvalidPrefixInStringLiteral, tempPosition, Stream.Position);
                            Tokens.Add(
                                new Token(
                                    TokenType.Identifier,
                                    tempPosition,
                                    c.ToString()
                                )
                            );
                            break;
                        }
                    }
                }

                bool stringHasPrefixes = strOptions.HasPrefixes;
                if (stringHasPrefixes) {
                    // remove last token - it is string prefix meant as identifier.
                    tokenStartPosition.column -= Tokens[Tokens.Count - 1].Value.Length;
                    Tokens.RemoveAt(Tokens.Count - 1);
                }

                delimiter        = c.ToString();
                strOptions.Quote = c;
                Stream.Move();
                if (c == strOptions.Quote) {
                    delimiter += c;
                    Stream.Move();
                    if (c == strOptions.Quote) {
                        delimiter += c;
                        Stream.Move();
                    }
                    else {
                        // only 2 quotes - an empty string.
                        Token emptyString = new StringToken(
                            tokenStartPosition,
                            strOptions,
                            ""
                        );
                        if (stringHasPrefixes) {
                            ReportWarning(WarningType.RedundantPrefixesForEmptyString, tokenStartPosition, Stream.Position);
                        }
                        Tokens.Add(emptyString);
                        return true;
                    }
                }
                if (delimiter.Length == 3) {
                    strOptions.IsMultiline = true;
                }
            }
            return false;
        }

        private void ReadStringInterpolation(ICollection<Interpolation> interpolations, int stringStartIndex, StringBuilder rawValue) {
            var newInterpolation = new Interpolation(Stream.CharIdx - stringStartIndex);
            interpolations.Add(newInterpolation);
            // process interpolation
            {
                var lexer = new Lexer(
                    Stream,
                    newInterpolation.Tokens,
                    Errors,
                    Warnings,
                    Options
                );
                lexer.AddPresets(processingTerminators: new[] { "}" });
                lexer.Stream.Move(); // skip {
                lexer._mismatchingPairs.Add(new Token(TokenType.OpLeftBrace, Stream.Position, "{"));
                lexer.Process();
                // remove usefulness closing curly
                newInterpolation.Tokens.RemoveAt(newInterpolation.Tokens.Count - 1);
                // restore character position
                Stream = lexer.Stream;
            }
            // append interpolated piece to main string token
            newInterpolation.EndIndex = Stream.CharIdx - stringStartIndex;
            string value = Stream.Source.Substring(Stream.CharIdx - newInterpolation.Length, newInterpolation.Length);
            rawValue.Append(value);
            tokenValue.Append(value);
        }
    }
}