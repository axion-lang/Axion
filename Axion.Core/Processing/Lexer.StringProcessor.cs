using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    public partial class Lexer {
        private Token ReadString(bool continueUnclosedString) {
            bool isStringEmpty = ReadStringPrefixes(
                ref continueUnclosedString,
                out StringLiteralOptions strOptions,
                out string delimiter,
                out char quote
            );

            if (isStringEmpty) {
                return null;
            }

            var trailingQuotes = "";
            var unescapedValue = new StringBuilder();
            var interpolations = new List<Interpolation>();
            int startIndex     = Stream.CharIdx;

            StringToken result;
            if (strOptions.IsFormatted) {
                result = new InterpolatedStringToken(
                    tokenStartPosition,
                    strOptions,
                    quote,
                    interpolations,
                    "",
                    ""
                );
            }
            else {
                result = new StringToken(
                    tokenStartPosition,
                    strOptions,
                    quote,
                    "",
                    ""
                );
            }

            void AddPiece(string piece) {
                tokenValue.Append(piece);
                unescapedValue.Append(piece);
            }

            while (true) {
                if (c == '\\') {
                    Stream.Move();
                    if (strOptions.IsRaw) {
                        AddPiece("\\");
                        continue;
                    }
                    unescapedValue.Append("\\");
                    ReadEscapeSequence(ref result);
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
                    result.Value          = tokenValue.ToString();
                    result.UnescapedValue = unescapedValue.ToString();
                    result.IsUnclosed     = true;
                    ReportError(ErrorType.UnclosedString, result);
                    _unclosedStrings.Add(result);
                    return result;
                }

                // got non-escaped quote
                if (c == quote && (tokenValue.Length == 0 || tokenValue[tokenValue.Length - 1] != '\\')) {
                    Stream.Move();
                    // ending "
                    if (delimiter.Length == 1) {
                        trailingQuotes = "";
                        break;
                    }
                    if (c == quote) {
                        Stream.Move();
                        if (c == quote) {
                            // ending """
                            trailingQuotes = "";
                            Stream.Move();
                            break;
                        }
                        trailingQuotes += quote;
                    }
                    // unescaped quote in string - error
                    result.Value          =  tokenValue.ToString();
                    result.UnescapedValue =  unescapedValue.ToString();
                    result.IsUnclosed     =  true;
                    trailingQuotes        += quote;
                    result.AddTrailingQuotes(trailingQuotes);
                    ReportError(ErrorType.UnescapedQuoteInStringLiteral, result);
                    return result;
                }

                // found string format sign
                if (c == '{' && strOptions.IsFormatted) {
                    ReadStringInterpolation(interpolations, startIndex);
                    continue;
                }
                // else
                AddPiece(c.ToString());
                Stream.Move();
            }
            if (strOptions.IsFormatted) {
                var interpolatedResult = (InterpolatedStringToken) result;
                interpolatedResult.Value          = tokenValue.ToString();
                interpolatedResult.UnescapedValue = unescapedValue.ToString();
                interpolatedResult.IsUnclosed     = false;
                interpolatedResult.AddTrailingQuotes(trailingQuotes);
                interpolatedResult.Interpolations.AddRange(interpolations);
                if (interpolations.Count == 0) {
                    ReportWarning(
                        WarningType.RedundantStringFormatPrefix,
                        interpolatedResult
                    );
                }
                return interpolatedResult;
            }
            result.Value          = tokenValue.ToString();
            result.UnescapedValue = unescapedValue.ToString();
            result.IsUnclosed     = false;
            result.AddTrailingQuotes(trailingQuotes);
            return result;
        }

        private bool ReadStringPrefixes(
            ref bool                 continueUnclosedString,
            out StringLiteralOptions stringLiteralOptions,
            out string               delimiter,
            out char                 quote
        ) {
            if (continueUnclosedString) {
                StringToken stringToken = _unclosedStrings.Last();
                _unclosedStrings.RemoveAt(_unclosedStrings.Count - 1);
                stringLiteralOptions = stringToken.Options;
                delimiter = new string(
                    stringToken.Quote,
                    stringLiteralOptions.QuotesCount
                );
                quote = stringToken.Quote;
            }
            else {
                stringLiteralOptions = new StringLiteralOptions();
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
                        stringLiteralOptions.AppendPrefix(
                            pfc,
                            out validPrefix,
                            out bool duplicatedPrefix
                        );
                        if (validPrefix) {
                            if (duplicatedPrefix) {
                                var tempId = new Token(
                                    TokenType.Identifier,
                                    tempPosition,
                                    c.ToString()
                                );
                                ReportWarning(
                                    WarningType.DuplicatedStringPrefix,
                                    tempId
                                );
                            }
                        }
                        // got invalid prefix letter or
                        // digit right before quote - error!
                        else if (char.IsLetterOrDigit(pfc)) {
                            var invalidPrefix = new Token(
                                TokenType.Identifier,
                                tempPosition,
                                c.ToString()
                            );
                            ReportError(
                                ErrorType.InvalidPrefixInStringLiteral,
                                invalidPrefix
                            );
                            Tokens.AddLast(invalidPrefix);
                            break;
                        }
                    }
                }

                bool stringHasPrefixes = stringLiteralOptions.HasPrefixes;
                if (stringHasPrefixes) {
                    // remove last token - it is string prefix meant as identifier.
                    tokenStartPosition.column -= Tokens.Last.Value.Value.Length;
                    Tokens.RemoveLast();
                }

                delimiter = c.ToString();
                quote     = c;
                Stream.Move();
                if (c == quote) {
                    delimiter += c;
                    Stream.Move();
                    if (c == quote) {
                        delimiter += c;
                        Stream.Move();
                    }
                    else {
                        // only 2 quotes - an empty string.
                        Token emptyString = new StringToken(
                            tokenStartPosition,
                            stringLiteralOptions,
                            quote,
                            ""
                        );
                        if (stringHasPrefixes) {
                            ReportWarning(
                                WarningType.RedundantPrefixesForEmptyString,
                                emptyString
                            );
                        }
                        Tokens.AddLast(emptyString);
                        return true;
                    }
                }
                if (delimiter.Length == 3) {
                    stringLiteralOptions.IsMultiline = true;
                }
            }
            return false;
        }

        private void ReadEscapeSequence(ref StringToken result) {
            // we're after '\'
            if (Spec.EscapeSequences.TryGetValue(c, out string sequence)) {
                tokenValue.Append(sequence);
                result.UnescapedValue += c;
                Stream.Move();
                return;
            }
            switch (c) {
                // unicode symbol
                case 'u':
                case 'U': {
                    char u             = c;
                    int  unicodeSymLen = u == 'u' ? 4 : 8;
                    Stream.Move();
                    if (LiteralParser.TryParseInt(Stream.Source, Stream.CharIdx, unicodeSymLen, 16, out int val)) {
                        if (val < 0 || val > 0x10ffff) {
                            ReportError(
                                ErrorType.IllegalUnicodeCharacter,
                                result
                            );
                        }
                        else if (val < 0x010000) {
                            tokenValue.Append(((char) val).ToString());
                        }
                        else {
                            tokenValue.Append(char.ConvertFromUtf32(val));
                        }
                        result.UnescapedValue += u + Stream.Source.Substring(
                                                     Stream.CharIdx,
                                                     unicodeSymLen
                                                 );
                        Stream.Move(unicodeSymLen);
                    }
                    else {
                        ReportError(
                            ErrorType.Truncated_uXXXX_Escape,
                            result
                        );
                    }
                    return;
                }
                // hexadecimal char
                case 'x' when LiteralParser.TryParseInt(Stream.Source, Stream.CharIdx, 2, 16, out int val): {
                    tokenValue.Append(((char) val).ToString());
                    result.UnescapedValue += Stream.Source.Substring(
                        Stream.CharIdx,
                        3
                    );
                    Stream.Move(3);
                    return;
                }
                case Spec.EndOfStream: {
                    return;
                }
                default: {
                    tokenValue.Append(c);
                    result.UnescapedValue += c;
                    return;
                }
            }
        }

        private void ReadStringInterpolation(ICollection<Interpolation> interpolations, int stringStartIndex) {
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
                newInterpolation.Tokens.RemoveLast();
                // restore character position
                Stream = new CharStream(lexer.Stream);
            }
            // append interpolated piece to main string token
            newInterpolation.EndIndex = Stream.CharIdx - stringStartIndex;
            tokenValue.Append(Stream.Source.Substring(Stream.CharIdx - newInterpolation.Length, newInterpolation.Length));
        }
    }
}