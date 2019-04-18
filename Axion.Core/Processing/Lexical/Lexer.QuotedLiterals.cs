using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        /// <summary>
        ///     Gets a character literal from next piece of source.
        /// </summary>
        private CharacterToken ReadCharLiteral() {
            var escapedValue = "";
            Move(); // eat opening quote
            var isUnclosed = false;
            while (true) {
                if (c == Spec.CharacterLiteralQuote) {
                    Move(); // eat closing quote
                    break;
                }

                if (AtEndOfLine) {
                    isUnclosed = true;
                    break;
                }

                if (c == Spec.EscapeMark) {
                    (string raw, string escaped) = ReadEscapeSequence();
                    tokenValue.Append(raw);
                    escapedValue += escaped;
                }
                else {
                    tokenValue.Append(c);
                    escapedValue += c;
                    Move();
                }
            }

            if (escapedValue.Length == 0) {
                unit.Blame(BlameType.EmptyCharacterLiteral, tokenStartPosition, Position);
            }
            else if (escapedValue.Length > 1) {
                unit.Blame(BlameType.CharacterLiteralTooLong, tokenStartPosition, Position);
            }

            if (isUnclosed) {
                unit.Blame(BlameType.UnclosedCharacterLiteral, tokenStartPosition, Position);
            }

            return new CharacterToken(
                tokenValue.ToString(),
                escapedValue,
                isUnclosed,
                tokenStartPosition
            );
        }

        #region String literal

        private StringToken? ReadString(bool continueUnclosedString) {
            bool isStringEmpty = ReadStringPrefixes(
                ref continueUnclosedString,
                out StringLiteralOptions strOptions,
                out string delimiter
            );

            if (isStringEmpty) {
                return null;
            }

            var escapedValue   = "";
            var interpolations = new List<Interpolation>();
            int startIndex     = charIdx;

            void AddPiece(string piece) {
                tokenValue.Append(piece);
                escapedValue += piece;
            }

            while (true) {
                if (c == Spec.EscapeMark && !strOptions.IsRaw) {
                    (string raw, string escaped) = ReadEscapeSequence();
                    tokenValue.Append(raw);
                    escapedValue += escaped;
                    continue;
                }

                // check for unclosed string
                if ((c == '\r' || c == '\n') && delimiter.Length == 1
                    || c == Spec.EndOfCode) {
                    var result = new StringToken(
                        strOptions,
                        tokenValue.ToString(),
                        escapedValue,
                        isUnclosed: true,
                        startPosition: tokenStartPosition
                    );
                    unclosedStrings.Add(result);
                    unit.Blame(BlameType.UnclosedString, tokenStartPosition, Position);
                    return result;
                }

                // found end of line
                if (c == '\r' && strOptions.IsLineEndsNormalized) {
                    Move();
                    // normalize line endings
                    if (c == '\n') {
                        Move();
                    }

                    AddPiece("\n");
                }

                // got non-escaped quote
                if (c == strOptions.Quote) {
                    Position startPosition = Position;
                    Move();
                    // ending "
                    if (delimiter.Length == 1) {
                        break;
                    }

                    var trailingQuotes = "";
                    if (c == strOptions.Quote) {
                        Move();
                        if (c == strOptions.Quote) {
                            // ending """
                            Move();
                            break;
                        }

                        trailingQuotes += strOptions.Quote;
                    }

                    trailingQuotes += strOptions.Quote;

                    // unescaped quote in string - error
                    var result = new StringToken(
                        strOptions,
                        tokenValue.ToString(),
                        escapedValue,
                        isUnclosed: true,
                        trailingQuotes: trailingQuotes,
                        startPosition: tokenStartPosition
                    );
                    unit.Blame(
                        BlameType.UnescapedQuoteInStringLiteral,
                        startPosition,
                        Position
                    );
                    return result;
                }

                // found string format sign
                if (c == '{'
                    && strOptions.IsFormatted) {
                    string val = ReadStringInterpolation(interpolations, startIndex);
                    tokenValue.Append(val);
                    escapedValue += val;
                    continue;
                }

                // else
                AddPiece(c.ToString());
                Move();
            }

            if (strOptions.IsFormatted && interpolations.Count == 0) {
                unit.Blame(
                    BlameType.RedundantStringFormatPrefix,
                    tokenStartPosition,
                    Position
                );
            }

            return new StringToken(
                strOptions,
                tokenValue.ToString(),
                escapedValue,
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
                    int                    tempCharIdx  = charIdx;
                    // read string prefixes
                    var validPrefix = true;
                    while (tempPosition.column > 0 && validPrefix) {
                        tempCharIdx--;
                        tempPosition.column--;
                        char pfc = unit.Code[tempCharIdx];
                        strOptions.AppendPrefix(pfc, out validPrefix, out bool duplicatedPrefix);
                        if (validPrefix) {
                            if (duplicatedPrefix) {
                                unit.Blame(
                                    BlameType.DuplicatedStringPrefix,
                                    tempPosition,
                                    Position
                                );
                            }
                        }
                        // got invalid prefix letter or
                        // digit right before quote - error!
                        else if (char.IsLetterOrDigit(pfc)) {
                            unit.Blame(
                                BlameType.InvalidPrefixInStringLiteral,
                                tempPosition,
                                Position
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
                        tokenStartPosition.Column - tokens.Last().Value.Length
                    );
                    tokens.RemoveAt(tokens.Count - 1);
                }

                delimiter        = c.ToString();
                strOptions.Quote = c;
                Move();
                if (c == strOptions.Quote) {
                    delimiter += c;
                    Move();
                    if (c == strOptions.Quote) {
                        delimiter += c;
                        Move();
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
                                Position
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

        private string ReadStringInterpolation(
            in ICollection<Interpolation> interpolations,
            int                           stringStartIndex
        ) {
            var newInterpolation = new Interpolation(charIdx - stringStartIndex);
            interpolations.Add(newInterpolation);
            // process interpolation
            {
                var lexer = new Lexer(unit, charIdx, lineIdx, columnIdx, newInterpolation.Tokens);
                lexer.processCancellers.Add("}");
                lexer.Move(); // skip '{'
                lexer.mismatchingPairs.Add(new SymbolToken(TokenType.OpenBrace));
                lexer.Process();

                // remove usefulness closing curly
                newInterpolation.Tokens.RemoveAt(newInterpolation.Tokens.Count - 1);

                // restore character position
                charIdx   = lexer.charIdx;
                lineIdx   = lexer.lineIdx;
                columnIdx = lexer.columnIdx;
            }
            // append interpolated piece to main string token
            newInterpolation.EndIndex = charIdx - stringStartIndex;
            return unit.Code.Substring(
                charIdx - newInterpolation.Length,
                newInterpolation.Length
            );
        }

        #endregion

        #region Escape sequences reading

        private (string raw, string escaped) ReadEscapeSequence() {
            var raw     = "";
            var escaped = "";
            // on \
            (int line, int column) escapePosition = Position;
            Move();
            // single-character sequence (\n, \t, etc.)
            if (Spec.EscapeSequences.TryGetValue(c, out string sequence)) {
                raw     = Spec.EscapeMark + "" + c;
                escaped = sequence;
                Move();
                return (raw, escaped);
            }

            switch (c) {
                // unicode character
                // 16 bits \u n n n n
                case 'u':
                // 32 bits \U(n n) n n n n n n
                case 'U': {
                    char u             = c;
                    int  unicodeSymLen = u == 'u' ? 4 : Spec.Unicode32BitHexLength;
                    Move();
                    var number = "";
                    var error  = false;
                    while (number.Length < unicodeSymLen) {
                        if (c.IsValidHexadecimalDigit()) {
                            number += c;
                            Move();
                        }
                        else if (number.Length < unicodeSymLen) {
                            unit.Blame(
                                BlameType.TruncatedEscapeSequence,
                                escapePosition,
                                Position
                            );
                            error = true;
                            break;
                        }
                    }

                    raw = Spec.EscapeMark + "" + u + number;

                    if (!error
                        && TryParseInt(number, 16, out int val)) {
                        if (val < 0
                            || val > 0x10ffff) {
                            unit.Blame(
                                BlameType.IllegalUnicodeCharacter,
                                escapePosition,
                                Position
                            );
                        }
                        else if (val < 0x010000) {
                            escaped = ((char) val).ToString();
                        }
                        else {
                            escaped = char.ConvertFromUtf32(val);
                        }
                    }
                    else {
                        escaped = raw;
                    }

                    break;
                }

                // TODO: Add \N{name} escape sequences
                // TODO: Add warnings for meaningless escapes, what can be shortened (e. g. \x00)
                // hexadecimal character \xn[n][n][n]
                case 'x': {
                    Move();
                    var number = "";
                    var error  = false;
                    while (c.IsValidHexadecimalDigit()
                           && number.Length < 4) {
                        number += c;
                        Move();
                    }

                    if (number.Length == 0) {
                        error = true;
                        unit.Blame(BlameType.InvalidXEscapeFormat, escapePosition, Position);
                    }

                    raw = Spec.EscapeMark + "x" + number;
                    if (!error
                        && TryParseInt(number, 16, out int val)) {
                        escaped = ((char) val).ToString();
                    }
                    else {
                        escaped = raw;
                    }

                    break;
                }

                // truncated escape & unclosed literal
                case '\n':
                case '\r':
                case Spec.EndOfCode: {
                    unit.Blame(BlameType.TruncatedEscapeSequence, escapePosition, Position);
                    raw = escaped = Spec.EscapeMark.ToString();
                    break;
                }

                // not a valid escape seq.
                default: {
                    unit.Blame(BlameType.InvalidEscapeSequence, escapePosition, Position);
                    raw = escaped = Spec.EscapeMark + "" + c;
                    Move();
                    break;
                }
            }

            return (raw, escaped);
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