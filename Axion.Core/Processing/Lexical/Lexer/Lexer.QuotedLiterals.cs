using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.Spec;

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
            stream.Move();
            switch (c) {
                case EscapeMark: {
                    // escape sequence
                    stream.Move();
                    (string value, string raw) = ReadEscapeSequence();
                    tokenValue.Append(value);
                    unescapedValue += raw;
                    break;
                }
                case CharacterLiteralQuote: {
                    unit.Blame(
                        BlameType.EmptyCharacterLiteral,
                        tokenStartPosition,
                        stream.Position
                    );
                    break;
                }
                default: {
                    // any character
                    tokenValue.Append(c);
                    stream.Move();
                    break;
                }
            }

            CharacterToken result;

            if (c != CharacterLiteralQuote) {
                unit.Blame(BlameType.CharacterLiteralTooLong, tokenStartPosition, stream.Position);
                while (c != CharacterLiteralQuote) {
                    if (stream.AtEndOfLine()
                        || c == EndOfStream) {
                        unit.Blame(
                            BlameType.UnclosedCharacterLiteral,
                            tokenStartPosition,
                            stream.Position
                        );
                        break;
                    }

                    tokenValue.Append(c);
                    unescapedValue += c;
                    stream.Move();
                }

                // can be or too long, or unclosed.
                result = new CharacterToken(
                    tokenValue.ToString(),
                    unescapedValue,
                    true,
                    tokenStartPosition
                );
            }
            else {
                // OK, valid literal
                result = new CharacterToken(
                    tokenValue.ToString(),
                    unescapedValue,
                    false,
                    tokenStartPosition
                );
                stream.Move();
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
            int startIndex     = stream.CharIdx;

            void AddPiece(string piece) {
                tokenValue.Append(piece);
                rawValue.Append(piece);
            }

            while (true) {
                if (c == EscapeMark) {
                    stream.Move();
                    if (strOptions.IsRaw) {
                        AddPiece(EscapeMark.ToString());
                        continue;
                    }

                    (string value, string raw) value = ReadEscapeSequence();
                    tokenValue.Append(value.value);
                    rawValue.Append(value.raw);
                    continue;
                }

                // found end of line
                if (c == '\r'
                    && strOptions.IsLineEndsNormalized) {
                    stream.Move();
                    // normalize line endings
                    if (c == '\n') {
                        stream.Move();
                    }

                    AddPiece("\n");
                }

                // check for end of line/stream
                StringToken result;
                if (c == EndOfStream
                    || stream.AtEndOfLine() && delimiter.Length == 1) {
                    result = new StringToken(
                        strOptions,
                        tokenValue.ToString(),
                        rawValue.ToString(),
                        isUnclosed: true,
                        startPosition: tokenStartPosition
                    );
                    unclosedStrings.Add(result);
                    unit.Blame(BlameType.UnclosedString, tokenStartPosition, stream.Position);
                    return result;
                }

                // got non-escaped quote
                if (c == strOptions.Quote) {
                    Position startPosition = stream.Position;
                    stream.Move();
                    // ending "
                    if (delimiter.Length == 1) {
                        strOptions.TrailingQuotes = "";
                        break;
                    }

                    if (c == strOptions.Quote) {
                        stream.Move();
                        if (c == strOptions.Quote) {
                            // ending """
                            strOptions.TrailingQuotes = "";
                            stream.Move();
                            break;
                        }

                        strOptions.TrailingQuotes += strOptions.Quote;
                    }

                    strOptions.TrailingQuotes += strOptions.Quote;

                    // unescaped quote in string - error
                    result = new StringToken(
                        strOptions,
                        tokenValue.ToString(),
                        rawValue.ToString(),
                        isUnclosed: true,
                        startPosition: tokenStartPosition
                    );
                    unit.Blame(
                        BlameType.UnescapedQuoteInStringLiteral,
                        startPosition,
                        stream.Position
                    );
                    return result;
                }

                // found string format sign
                if (c == '{'
                    && strOptions.IsFormatted) {
                    ReadStringInterpolation(interpolations, startIndex, rawValue);
                    continue;
                }

                // else
                AddPiece(c.ToString());
                stream.Move();
            }

            if (strOptions.IsFormatted) {
                if (interpolations.Count == 0) {
                    unit.Blame(
                        BlameType.RedundantStringFormatPrefix,
                        tokenStartPosition,
                        stream.Position
                    );
                }
            }

            return new StringToken(
                strOptions,
                tokenValue.ToString(),
                rawValue.ToString(),
                interpolations,
                startPosition: tokenStartPosition
            );
        }

        private bool ReadStringPrefixes(
            ref bool                 continueUnclosedString,
            out StringLiteralOptions strOptions,
            out string               delimiter
        ) {
            if (continueUnclosedString) {
                StringToken stringToken = unclosedStrings.Last();
                unclosedStrings.RemoveAt(unclosedStrings.Count - 1);
                strOptions = stringToken.Options;
                delimiter  = new string(stringToken.Options.Quote, strOptions.QuotesCount);
            }
            else {
                strOptions = new StringLiteralOptions();
                {
                    // get letters before the string, process prefixes
                    (int line, int column) tempPosition = tokenStartPosition;
                    int                    tempCharIdx  = stream.CharIdx;
                    // read string prefixes
                    var validPrefix = true;
                    while (tempPosition.column > 0 && validPrefix) {
                        tempCharIdx--;
                        tempPosition.column--;
                        char pfc = stream.Source[tempCharIdx];
                        strOptions.AppendPrefix(pfc, out validPrefix, out bool duplicatedPrefix);
                        if (validPrefix) {
                            if (duplicatedPrefix) {
                                unit.Blame(
                                    BlameType.DuplicatedStringPrefix,
                                    tempPosition,
                                    stream.Position
                                );
                            }
                        }
                        // got invalid prefix letter or
                        // digit right before quote - error!
                        else if (char.IsLetterOrDigit(pfc)) {
                            unit.Blame(
                                BlameType.InvalidPrefixInStringLiteral,
                                tempPosition,
                                stream.Position
                            );
                            tokens.Add(new Token(TokenType.Identifier, c.ToString(), tempPosition));
                            break;
                        }
                    }
                }

                bool stringHasPrefixes = strOptions.HasPrefixes;
                if (stringHasPrefixes) {
                    // remove last token - it is string prefix meant as identifier.
                    tokenStartPosition = new Position(
                        tokenStartPosition.Line,
                        tokenStartPosition.Column - tokens[tokens.Count - 1].Value.Length
                    );
                    tokens.RemoveAt(tokens.Count - 1);
                }

                delimiter        = c.ToString();
                strOptions.Quote = c;
                stream.Move();
                if (c == strOptions.Quote) {
                    delimiter += c;
                    stream.Move();
                    if (c == strOptions.Quote) {
                        delimiter += c;
                        stream.Move();
                    }
                    else {
                        // only 2 quotes - an empty string.
                        Token emptyString = new StringToken(
                            strOptions,
                            "",
                            startPosition: tokenStartPosition
                        );
                        if (stringHasPrefixes) {
                            unit.Blame(
                                BlameType.RedundantPrefixesForEmptyString,
                                tokenStartPosition,
                                stream.Position
                            );
                        }

                        tokens.Add(emptyString);
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
            var newInterpolation = new Interpolation(stream.CharIdx - stringStartIndex);
            interpolations.Add(newInterpolation);
            // process interpolation
            {
                var lexer = new Lexer(unit, stream, newInterpolation.Tokens);
                lexer.AddPresets(
                    processTerminators_: new[] {
                        "}"
                    }
                );
                lexer.stream.Move(); // skip '{'
                lexer.mismatchingPairs.Add(new Token(TokenType.LeftBrace, "{", stream.Position));
                lexer.Process();
                // remove usefulness closing curly
                newInterpolation.Tokens.RemoveAt(newInterpolation.Tokens.Count - 1);
                // restore character position
                stream = lexer.stream;
            }
            // append interpolated piece to main string token
            newInterpolation.EndIndex = stream.CharIdx - stringStartIndex;
            string value = stream.Source.Substring(
                stream.CharIdx - newInterpolation.Length,
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
            (int line, int column)     startPosition = stream.Position;
            startPosition.column--;
            // single-character sequence (\n, \t, etc.)
            if (EscapeSequences.TryGetValue(c, out string sequence)) {
                result.value = sequence;
                result.raw   = EscapeMark + "" + c;
                stream.Move();
                return result;
            }

            switch (c) {
                // unicode character
                // 16 bits \u n n n n
                case 'u':
                // 32 bits \U(n n) n n n n n n
                case 'U': {
                    char u             = c;
                    int  unicodeSymLen = u == 'u' ? 4 : Unicode32BitHexLength;
                    stream.Move();
                    var number = "";
                    var error  = false;
                    while (number.Length < unicodeSymLen) {
                        if (c.IsValidHexadecimalDigit()) {
                            number += c;
                            stream.Move();
                        }
                        else if (number.Length < unicodeSymLen) {
                            unit.Blame(
                                BlameType.TruncatedEscapeSequence,
                                startPosition,
                                stream.Position
                            );
                            error = true;
                            break;
                        }
                    }

                    result.raw += EscapeMark + u + number;

                    if (!error
                        && TryParseInt(number, 16, out int val)) {
                        if (val < 0
                            || val > 0x10ffff) {
                            unit.Blame(
                                BlameType.IllegalUnicodeCharacter,
                                startPosition,
                                stream.Position
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
                    stream.Move();
                    var number = "";
                    var error  = false;
                    while (c.IsValidHexadecimalDigit()
                           && number.Length < 4) {
                        number += c;
                        stream.Move();
                    }

                    if (number.Length == 0) {
                        error = true;
                        unit.Blame(BlameType.InvalidXEscapeFormat, startPosition, stream.Position);
                    }

                    result.raw = "\\x" + number;
                    if (!error
                        && TryParseInt(number, 16, out int val)) {
                        result.value += ((char) val).ToString();
                    }
                    else {
                        result.value += result.raw;
                    }

                    return result;
                }
                // truncated escape & unclosed literal
                case EndOfStream: {
                    result.value += EscapeMark;
                    result.raw   += EscapeMark;
                    unit.Blame(BlameType.TruncatedEscapeSequence, startPosition, stream.Position);
                    return result;
                }
                // not a valid escape seq.
                default: {
                    unit.Blame(BlameType.InvalidEscapeSequence, startPosition, stream.Position);
                    result.value += EscapeMark + "" + c;
                    result.raw   += EscapeMark + "" + c;
                    return result;
                }
            }
        }

        private static bool TryParseInt(string input, int radix, out int value) {
            value = 0;
            foreach (char с in input) {
                if (HexValue(с, out int oneChar)
                    && oneChar < radix) {
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
                    if (ch >= 'a'
                        && ch <= 'z') {
                        value = ch - 'a' + 10;
                    }
                    else if (ch >= 'A'
                             && ch <= 'Z') {
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