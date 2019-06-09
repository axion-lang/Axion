using System.Globalization;
using System.Linq;
using System.Numerics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        private NumberToken ReadNumberStart() {
            NumberOptions nOptions;
            if (c == '0') {
                tokenValue.Append("0");
                Move();

                var isOnBaseLetter = false;
                switch (c) {
                // on second char (base letter, determines radix)
                case 'b':
                case 'B':
                    tokenValue.Append(c);
                    Move();
                    nOptions = ReadNumberValue(2, out isOnBaseLetter);
                    break;
                case 'o':
                case 'O':
                    tokenValue.Append(c);
                    Move();
                    nOptions = ReadNumberValue(8, out isOnBaseLetter);
                    break;
                case 'x':
                case 'X':
                    tokenValue.Append(c);
                    Move();
                    nOptions = ReadNumberValue(16, out isOnBaseLetter);
                    break;
                default: {
                    // regular num with 0's at beginning
                    // skip leading zeros
                    while (c == '0') {
                        tokenValue.Append("0");
                        Move();
                    }

                    nOptions = ReadNumberValue(10, out bool _);
                    nOptions.ClearNumber.Insert(0, "0");
                    break;
                }
                }

                // '0x', '0b', '0o'
                if (isOnBaseLetter) {
                    unit.Blame(
                        BlameType.ExpectedNumberValueAfterNumberBaseSpecifier,
                        tokenStartPosition,
                        Position
                    );
                }
            }
            else {
                nOptions = ReadNumberValue(10, out bool _);
            }

            return new NumberToken(tokenValue.ToString(), nOptions, tokenStartPosition);
        }

        private NumberOptions ReadNumberValue(in int radix, out bool isOnBaseLetter) {
            var nOptions = new NumberOptions(radix);
            isOnBaseLetter = true;
            for (; Spec.IsLetterOrNumberPart(c); Move()) {
                if (int.TryParse(c.ToString(), NumberStyles.HexNumber, null, out int digit)
                    && digit < radix) {
                    nOptions.ClearNumber.Append(c);
                }
                else if (radix == 10 && c == '.') {
                    Position dotPosition = Position;
                    // for smth like '10.isOdd'
                    if (!char.IsDigit(Peek)) {
                        break;
                    }

                    if (nOptions.Floating) {
                        tokenValue.Append(c);
                        Move();
                        unit.Blame(
                            BlameType.RepeatedDotInNumberLiteral,
                            dotPosition,
                            Position + (0, 1)
                        );
                    }

                    nOptions.Floating = true;
                    nOptions.ClearNumber.Append(c);
                }
                // we support exponent only for radix <= 10
                else if (radix == 10 && (c == 'e' || c == 'E')) {
                    ReadNumExponent(nOptions);
                    break;
                }
                else if (Spec.NumberPostfixes.Contains(c)) {
                    ReadNumPostfix(nOptions, false);
                    return nOptions;
                }
                else if (c != '_') {
                    // invalid
                    unit.Blame(Spec.InvalidNumberLiteralError(radix), tokenStartPosition, Position);
                }

                tokenValue.Append(c);
                isOnBaseLetter = false;
            }

            return nOptions;
        }

        private void ReadNumExponent(in NumberOptions nOptions) {
            Position ePosition = Position;
            // c == 'e'
            // check for '0'
            string num  = tokenValue.ToString().Replace("_", "").TrimStart('0');
            bool   zero = tokenValue.Length > 0 && (num == "" || num == ".");

            nOptions.HasExponent = true;
            var hasValue   = false;
            var hasPostfix = false;
            tokenValue.Append(c);
            nOptions.ClearNumber.Append(c);
            Move();

            if (zero) {
                unit.Blame(BlameType.RedundantExponentForZeroNumber, ePosition, Position);
            }

            var eValue = "";
            if (c == '-' || c == '+') {
                eValue += c;
                tokenValue.Append(c);
                nOptions.ClearNumber.Append(c);
                Move();
            }

            for (; Spec.IsLetterOrNumberPart(c); Move()) {
                if (char.IsDigit(c)) {
                    hasValue =  true;
                    eValue   += c;
                    nOptions.ClearNumber.Append(c);
                }
                else if (Spec.NumberPostfixes.Contains(c)) {
                    hasPostfix = true;
                    break;
                }
                else if (c != '_') {
                    // invalid
                    unit.Blame(
                        BlameType.ExpectedNumberAfterExponentSign,
                        ePosition,
                        Position
                    );
                    break;
                }

                tokenValue.Append(c);
            }

            if (!hasValue) {
                unit.Blame(BlameType.ExpectedNumberAfterExponentSign, ePosition, Position);
            }
            else {
                nOptions.Exponent = int.Parse(eValue, NumberStyles.AllowLeadingSign);
            }

            if (hasPostfix) {
                ReadNumPostfix(nOptions, false);
            }
        }

        private void ReadNumPostfix(in NumberOptions nOptions, bool isOnBaseLetter) {
            Position postfixPosition = Position;
            // c is letter here
            if (isOnBaseLetter) {
                unit.Blame(
                    BlameType.ExpectedNumberValueAfterNumberBaseSpecifier,
                    tokenStartPosition,
                    Position
                );
            }

            ReadNumPostfixLetters(
                nOptions,
                out bool needEndOfNumber,
                out bool needBitRate
            );

            if (Spec.IsLetterOrNumberPart(c) && needEndOfNumber) {
                unit.Blame(
                    BlameType.ExpectedEndOfNumberAfterPostfix,
                    postfixPosition,
                    Position
                );
            }
            else if (char.IsDigit(c)) {
                Position bitRatePosition = Position;
                // Reading number bit rate
                var bitRateStr = "";
                while (char.IsDigit(c)) {
                    tokenValue.Append(c);
                    bitRateStr += c;
                    Move();
                }

                nOptions.Bits = int.Parse(bitRateStr);

                // check for invalid bit rates
                if (nOptions.Floating) {
                    if (!Spec.FloatBitRates.Contains(nOptions.Bits)) {
                        unit.Blame(
                            BlameType.InvalidFloatNumberBitRate,
                            bitRatePosition,
                            Position
                        );
                    }
                }
                else if (!Spec.IntegerBitRates.Contains(nOptions.Bits)) {
                    unit.Blame(
                        BlameType.InvalidIntegerNumberBitRate,
                        bitRatePosition,
                        Position
                    );
                }

                if (char.IsLetter(c)
                    || c == '_') {
                    unit.Blame(
                        BlameType.ExpectedEndOfNumberAfterPostfix,
                        postfixPosition,
                        Position
                    );
                }
            }
            else if (needBitRate) {
                // expected digit after num 'i#' postfix
                unit.Blame(
                    BlameType.ExpectedABitRateAfterNumberPostfix,
                    postfixPosition,
                    Position
                );
            }
        }

        private void ReadNumPostfixLetters(
            in  NumberOptions nOptions,
            out bool          needEndOfNumber,
            out bool          needBitRate
        ) {
            needEndOfNumber = false;
            needBitRate     = false;
            if (c == 'i' || c == 'I') {
                nOptions.Unsigned = false;
                needBitRate       = true;
            }
            else if (c == 'f' || c == 'F') {
                nOptions.Floating = true;
            }
            else if (c == 'u' || c == 'U') {
                nOptions.Unsigned = true;
            }
            else if (c == 'l' || c == 'L') {
                nOptions.Unlimited = true;
                if (!BigInteger.TryParse(
                    nOptions.ClearNumber.ToString(),
                    out BigInteger _
                )) {
                    unit.Blame(
                        BlameType.InvalidNumberLiteral,
                        tokenStartPosition,
                        Position
                    );
                }

                needEndOfNumber = true;
            }
            else if (c == 'j' || c == 'J') {
                nOptions.Imaginary = true;
                if (!double.TryParse(nOptions.ClearNumber.ToString(), out double _)) {
                    unit.Blame(
                        BlameType.InvalidComplexNumberLiteral,
                        tokenStartPosition,
                        Position
                    );
                }

                needEndOfNumber = true;
            }
            else {
                unit.Blame(
                    BlameType.InvalidPostfixInNumberLiteral,
                    tokenStartPosition,
                    Position
                );
                return;
            }

            tokenValue.Append(c);
            Move();
        }
    }
}