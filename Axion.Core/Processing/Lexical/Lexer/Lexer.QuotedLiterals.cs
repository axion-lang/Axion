using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        /// <summary>
        ///     Gets a character literal from next piece of source.
        ///     Returns valid char literal if next piece of source
        ///     is either validly escape sequence or just a char
        ///     with length of 1.
        ///     Otherwise, returns error token,
        ///     that can be either too long char literal,
        ///     or just unclosed char literal.
        /// </summary>
        private CharacterToken ReadCharLiteral() {
            var unescapedValue = "";
            // we're on char quote
            Stream.Move();
            switch (c) {
                case '\\': {
                    // escape sequence
                    Stream.Move();
                    (string value, string raw) = ReadEscapeSequence();
                    tokenValue.Append(value);
                    unescapedValue += raw;
                    break;
                }
                case Spec.CharLiteralQuote: {
                    Blame(BlameType.EmptyCharacterLiteral, tokenStartPosition, Stream.Position);
                    break;
                }
                default: {
                    // any character
                    tokenValue.Append(c);
                    Stream.Move();
                    break;
                }
            }

            CharacterToken result;

            if (c != Spec.CharLiteralQuote) {
                Blame(BlameType.CharacterLiteralTooLong, tokenStartPosition, Stream.Position);
                while (c != Spec.CharLiteralQuote) {
                    if (Stream.AtEndOfLine() || c == Spec.EndOfStream) {
                        Blame(
                            BlameType.UnclosedCharacterLiteral,
                            tokenStartPosition,
                            Stream.Position
                        );
                        break;
                    }
                    tokenValue.Append(c);
                    unescapedValue += c;
                    Stream.Move();
                }

                // can be or too long, or unclosed.
                result = new CharacterToken(
                    tokenStartPosition,
                    tokenValue.ToString(),
                    unescapedValue,
                    true
                );
            }
            else {
                // OK, valid literal
                result = new CharacterToken(
                    tokenStartPosition,
                    tokenValue.ToString(),
                    unescapedValue
                );
                Stream.Move();
            }
            return result;
        }

        #region String literal

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
                    Stream.Move();
                    // normalize line endings
                    if (c == '\n') {
                        Stream.Move();
                    }
                    AddPiece("\n");
                }

                // check for end of line/stream
                StringToken result;
                if (c == Spec.EndOfStream || Stream.AtEndOfLine() && delimiter.Length == 1) {
                    result = new StringToken(
                        tokenStartPosition,
                        strOptions,
                        tokenValue.ToString(),
                        rawValue.ToString(),
                        isUnclosed: true
                    );
                    _unclosedStrings.Add(result);
                    Blame(BlameType.UnclosedString, tokenStartPosition, Stream.Position);
                    return result;
                }

                // got non-escaped quote
                if (c == strOptions.Quote) {
                    Position startPosition = Stream.Position;
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
                        isUnclosed: true
                    );
                    Blame(BlameType.UnescapedQuoteInStringLiteral, startPosition, Stream.Position);
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
                    Blame(
                        BlameType.RedundantStringFormatPrefix,
                        tokenStartPosition,
                        Stream.Position
                    );
                }
            }
            return new StringToken(
                tokenStartPosition,
                strOptions,
                tokenValue.ToString(),
                rawValue.ToString(),
                interpolations
            );
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
                delimiter  = new string(stringToken.Options.Quote, strOptions.QuotesCount);
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
                        strOptions.AppendPrefix(pfc, out validPrefix, out bool duplicatedPrefix);
                        if (validPrefix) {
                            if (duplicatedPrefix) {
                                Blame(
                                    BlameType.DuplicatedStringPrefix,
                                    tempPosition,
                                    Stream.Position
                                );
                            }
                        }
                        // got invalid prefix letter or
                        // digit right before quote - error!
                        else if (char.IsLetterOrDigit(pfc)) {
                            Blame(
                                BlameType.InvalidPrefixInStringLiteral,
                                tempPosition,
                                Stream.Position
                            );
                            Tokens.Add(new Token(TokenType.Identifier, tempPosition, c.ToString()));
                            break;
                        }
                    }
                }

                bool stringHasPrefixes = strOptions.HasPrefixes;
                if (stringHasPrefixes) {
                    // remove last token - it is string prefix meant as identifier.
                    tokenStartPosition = new Position(
                        tokenStartPosition.Line,
                        tokenStartPosition.Column - Tokens[Tokens.Count - 1].Value.Length
                    );
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
                        Token emptyString = new StringToken(tokenStartPosition, strOptions, "");
                        if (stringHasPrefixes) {
                            Blame(
                                BlameType.RedundantPrefixesForEmptyString,
                                tokenStartPosition,
                                Stream.Position
                            );
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

        private void ReadStringInterpolation(
            in ICollection<Interpolation> interpolations,
            int                           stringStartIndex,
            in StringBuilder              rawValue
        ) {
            var newInterpolation = new Interpolation(Stream.CharIdx - stringStartIndex);
            interpolations.Add(newInterpolation);
            // process interpolation
            {
                var lexer = new Lexer(Stream, newInterpolation.Tokens, Blames, Options);
                lexer.AddPresets(processTerminators: new[] { "}" });
                lexer.Stream.Move(); // skip '{'
                lexer._mismatchingPairs.Add(new Token(TokenType.LeftBrace, Stream.Position, "{"));
                lexer.Process();
                // remove usefulness closing curly
                newInterpolation.Tokens.RemoveAt(newInterpolation.Tokens.Count - 1);
                // restore character position
                Stream = lexer.Stream;
            }
            // append interpolated piece to main string token
            newInterpolation.EndIndex = Stream.CharIdx - stringStartIndex;
            string value = Stream.Source.Substring(
                Stream.CharIdx - newInterpolation.Length,
                newInterpolation.Length
            );
            rawValue.Append(value);
            tokenValue.Append(value);
        }

        #endregion

        #region Escape sequences reading

        private (string value, string raw) ReadEscapeSequence() {
            // position is after \
            (string value, string raw) result        = ("", "");
            (int line, int column)     startPosition = Stream.Position;
            startPosition.column--;
            // single-character sequence (\n, \t, etc.)
            if (Spec.EscapeSequences.TryGetValue(c, out string sequence)) {
                result.value = sequence;
                result.raw   = "\\" + c;
                Stream.Move();
                return result;
            }
            switch (c) {
                // unicode character
                // 16 bits \u n n n n
                case 'u':
                // 32 bits \U(n n) n n n n n n
                case 'U': {
                    char u             = c;
                    int  unicodeSymLen = u == 'u' ? 4 : Spec.Unicode32BitHexLength;
                    Stream.Move();
                    var number = "";
                    var error  = false;
                    while (number.Length < unicodeSymLen) {
                        if (c.IsValidHexadecimalDigit()) {
                            number += c;
                            Stream.Move();
                        }
                        else if (number.Length < unicodeSymLen) {
                            Blame(
                                BlameType.TruncatedEscapeSequence,
                                startPosition,
                                Stream.Position
                            );
                            error = true;
                            break;
                        }
                    }
                    result.raw += "\\" + u + number;

                    if (!error && TryParseInt(number, 16, out int val)) {
                        if (val < 0 || val > 0x10ffff) {
                            Blame(
                                BlameType.IllegalUnicodeCharacter,
                                startPosition,
                                Stream.Position
                            );
                        }
                        else if (val < 0x010000) {
                            result.value += ((char) val).ToString();
                        }
                        else {
                            result.value += char.ConvertFromUtf32(val);
                        }
                    }
                    else {
                        result.value += result.raw;
                    }
                    return result;
                }
                // TODO: Add \N{name} escape sequences
                // TODO: Add warnings for meaningless escapes, what can be shortened (e. g. \x00)
                // hexadecimal character \xn[n][n][n]
                case 'x': {
                    Stream.Move();
                    var number = "";
                    var error  = false;
                    while (c.IsValidHexadecimalDigit() && number.Length < 4) {
                        number += c;
                        Stream.Move();
                    }
                    if (number.Length == 0) {
                        error = true;
                        Blame(BlameType.InvalidXEscapeFormat, startPosition, Stream.Position);
                    }

                    result.raw = "\\x" + number;
                    if (!error && TryParseInt(number, 16, out int val)) {
                        result.value += ((char) val).ToString();
                    }
                    else {
                        result.value += result.raw;
                    }
                    return result;
                }
                // truncated escape & unclosed literal
                case Spec.EndOfStream: {
                    result.value += '\\';
                    result.raw   += '\\';
                    Blame(BlameType.TruncatedEscapeSequence, startPosition, Stream.Position);
                    return result;
                }
                // not a valid escape seq.
                default: {
                    Blame(BlameType.InvalidEscapeSequence, startPosition, Stream.Position);
                    result.value += "\\" + c;
                    result.raw   += "\\" + c;
                    return result;
                }
            }
        }

        private static bool TryParseInt(string input, int radix, out int value) {
            value = 0;
            foreach (char с in input) {
                if (HexValue(с, out int oneChar) && oneChar < radix) {
                    value = value * radix + oneChar;
                }
                else {
                    return false;
                }
            }
            return true;
        }

        private static bool HexValue(char ch, out int value) {
            switch (ch) {
                case '0':
                case '\x660': {
                    value = 0;
                    break;
                }
                case '1':
                case '\x661': {
                    value = 1;
                    break;
                }
                case '2':
                case '\x662': {
                    value = 2;
                    break;
                }
                case '3':
                case '\x663': {
                    value = 3;
                    break;
                }
                case '4':
                case '\x664': {
                    value = 4;
                    break;
                }
                case '5':
                case '\x665': {
                    value = 5;
                    break;
                }
                case '6':
                case '\x666': {
                    value = 6;
                    break;
                }
                case '7':
                case '\x667': {
                    value = 7;
                    break;
                }
                case '8':
                case '\x668': {
                    value = 8;
                    break;
                }
                case '9':
                case '\x669': {
                    value = 9;
                    break;
                }
                default: {
                    if (ch >= 'a' && ch <= 'z') {
                        value = ch - 'a' + 10;
                    }
                    else if (ch >= 'A' && ch <= 'Z') {
                        value = ch - 'A' + 10;
                    }
                    else {
                        value = -1;
                        return false;
                    }
                    break;
                }
            }
            return true;
        }

        #endregion
    }
}