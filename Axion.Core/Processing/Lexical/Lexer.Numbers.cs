using System.Linq;
using System.Numerics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        private NumberToken ReadNumber() {
            NumberOptions numberOptions;
            if (c == '0') {
                tokenValue.Append("0");
                Move();

                bool isOnBaseLetter;
                switch (c) {
                    // on second char (base letter, determines radix)
                    case 'b':
                    case 'B':
                        tokenValue.Append(c);
                        Move();
                        numberOptions = ReadBinaryNumber(out isOnBaseLetter);
                        break;
                    case 'o':
                    case 'O':
                        tokenValue.Append(c);
                        Move();
                        numberOptions = ReadOctalNumber(out isOnBaseLetter);
                        break;
                    case 'x':
                    case 'X':
                        tokenValue.Append(c);
                        Move();
                        numberOptions = ReadHexNumber(out isOnBaseLetter);
                        break;
                    default: {
                        // regular num with 0's at beginning
                        isOnBaseLetter = false;
                        // skip leading zeros
                        while (c == '0') {
                            tokenValue.Append("0");
                            Move();
                        }

                        numberOptions = ReadDecimalNumber(true);
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
                // c in (1..9)
                numberOptions = ReadDecimalNumber(false);
            }

            return new NumberToken(tokenValue.ToString(), numberOptions, tokenStartPosition);
        }

        #region Dec, Bin, Oct, Hex

        private NumberOptions ReadDecimalNumber(bool startsWithZero) {
            var numberOptions = new NumberOptions {
                Radix = 10
            };
            if (startsWithZero) {
                numberOptions.Number.Append("0");
            }

            // c is digit or dot except 0 here
            while (Spec.IsLetterOrNumberPart(c)) {
                if (char.IsDigit(c)) {
                    numberOptions.Number.Append(c);
                }
                else if (c != '_') {
                    if (c == '.') {
                        Position dotPosition = Position;
                        if (!char.IsDigit(Peek)) {
                            break;
                        }

                        // if found second dot in number
                        if (numberOptions.Floating) {
                            tokenValue.Append(c);
                            Move();
                            unit.Blame(
                                BlameType.RepeatedDotInNumberLiteral,
                                dotPosition,
                                Position
                            );
                        }
                        else {
                            numberOptions.Number.Append(c);
                        }

                        numberOptions.Floating = true;
                    }
                    else if (c == 'e'
                             || c == 'E') {
                        ReadExponent(numberOptions);
                        continue;
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(numberOptions, false);
                        return numberOptions;
                    }
                    else {
                        // invalid
                        unit.Blame(
                            BlameType.InvalidNumberLiteral,
                            tokenStartPosition,
                            Position
                        );
                    }
                }

                tokenValue.Append(c);
                Move();
            }

            return numberOptions;
        }

        private NumberOptions ReadBinaryNumber(out bool isOnBaseLetter) {
            var numberOptions = new NumberOptions {
                Radix = 2
            };
            var        bitsCount = 0;
            var        longValue = 0L;
            BigInteger bigInt    = BigInteger.Zero;
            isOnBaseLetter = true;
            while (Spec.IsLetterOrNumberPart(c)) {
                switch (c) {
                    case '0': {
                        if (longValue != 0L) {
                            goto case '1';
                        }

                        break;
                    }

                    case '1': {
                        numberOptions.Number.Append(c);
                        bitsCount++;
                        if (bitsCount > 8) {
                            numberOptions.Bits = 16;
                        }
                        else if (bitsCount > 16) {
                            numberOptions.Bits = 32;
                        }
                        else if (bitsCount > 32) {
                            numberOptions.Bits = 64;
                        }
                        else if (bitsCount > 64) {
                            numberOptions.Bits = 128;
                            bigInt             = longValue;
                        }

                        // TODO debug
                        var or = (byte) (c - '0');
                        if (bitsCount >= 64) {
                            bigInt = (bigInt << 1) | or;
                        }
                        else {
                            longValue = (longValue << 1) | or;
                        }

                        break;
                    }

                    case '_': {
                        break;
                    }

                    default: {
                        if (Spec.NumberPostfixes.Contains(c)) {
                            ReadNumberPostfix(numberOptions, false);
                            return numberOptions;
                        }

                        // invalid
                        unit.Blame(
                            BlameType.InvalidBinaryLiteral,
                            tokenStartPosition,
                            Position
                        );
                        break;
                    }
                }

                tokenValue.Append(c);
                Move();
                isOnBaseLetter = false;
            }

            return numberOptions;
        }

        private NumberOptions ReadOctalNumber(out bool isOnBaseLetter) {
            var numberOptions = new NumberOptions {
                Radix = 8
            };
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (CharIs(Spec.OctalDigits)) {
                    numberOptions.Number.Append(c);
                }
                else if (c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(numberOptions, isOnBaseLetter);
                        break;
                    }

                    // invalid
                    unit.Blame(BlameType.InvalidOctalLiteral, tokenStartPosition, Position);
                }

                tokenValue.Append(c);
                Move();
                isOnBaseLetter = false;
            }

            return numberOptions;
        }

        private NumberOptions ReadHexNumber(out bool isOnBaseLetter) {
            var numberOptions = new NumberOptions {
                Radix = 16
            };
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (CharIs(Spec.HexadecimalDigits)) {
                    numberOptions.Number.Append(c);
                }
                else if (c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(numberOptions, isOnBaseLetter);
                        break;
                    }

                    // invalid
                    unit.Blame(
                        BlameType.InvalidHexadecimalLiteral,
                        tokenStartPosition,
                        Position
                    );
                }

                tokenValue.Append(c);
                Move();
                isOnBaseLetter = false;
            }

            return numberOptions;
        }

        private void ReadExponent(in NumberOptions numberOptions) {
            Position ePosition = Position;
            // c == 'e'
            // check for '0'
            string num  = tokenValue.ToString().Replace("_", "").Trim('0');
            bool   zero = tokenValue.Length > 0 && (num == "" || num == ".");

            numberOptions.HasExponent = true;
            var hasValue   = false;
            var hasPostfix = false;
            tokenValue.Append(c);
            Move();

            if (zero) {
                unit.Blame(BlameType.RedundantExponentForZeroNumber, ePosition, Position);
            }

            string eValue = c == '-' ? "-" : "";
            if (c == '-' || c == '+') {
                tokenValue.Append(c);
                Move();
            }

            while (Spec.IsLetterOrNumberPart(c)) {
                if (char.IsDigit(c)) {
                    hasValue =  true;
                    eValue   += c;
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
                Move();
            }

            if (!hasValue) {
                unit.Blame(BlameType.ExpectedNumberAfterExponentSign, ePosition, Position);
            }
            else {
                numberOptions.Exponent = int.Parse(eValue);
            }

            if (hasPostfix) {
                ReadNumberPostfix(numberOptions, false);
            }
        }

        #endregion

        #region Postfixes

        private void ReadNumberPostfix(in NumberOptions numberOptions, bool isOnBaseLetter) {
            Position postfixPosition = Position;
            // c is letter here
            if (isOnBaseLetter) {
                unit.Blame(
                    BlameType.ExpectedNumberValueAfterNumberBaseSpecifier,
                    tokenStartPosition,
                    Position
                );
            }

            ReadNumberPostfixLetters(
                numberOptions,
                out bool expectingEndOfNumber,
                out bool bitRateRequired
            );

            if (Spec.IsLetterOrNumberPart(c) && expectingEndOfNumber) {
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

                numberOptions.Bits = int.Parse(bitRateStr);

                // check for invalid bit rates
                if (numberOptions.Floating) {
                    if (!Spec.FloatBitRates.Contains(numberOptions.Bits)) {
                        unit.Blame(
                            BlameType.InvalidFloatNumberBitRate,
                            bitRatePosition,
                            Position
                        );
                    }
                }
                else if (!Spec.IntegerBitRates.Contains(numberOptions.Bits)) {
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
            else if (bitRateRequired) {
                // expected digit after num 'i#' postfix
                unit.Blame(
                    BlameType.ExpectedABitRateAfterNumberPostfix,
                    postfixPosition,
                    Position
                );
            }
        }

        private void ReadNumberPostfixLetters(
            in  NumberOptions numberOptions,
            out bool          expectingEndOfNumber,
            out bool          bitRateRequired
        ) {
            expectingEndOfNumber = true;
            bitRateRequired      = false;
            switch (c) {
                // read postfix
                // Number limits
                case 'i':
                case 'I': {
                    numberOptions.Unsigned = false;
                    expectingEndOfNumber   = false;
                    bitRateRequired        = true;
                    break;
                }

                case 'f':
                case 'F': {
                    numberOptions.Floating = true;
                    expectingEndOfNumber   = false;
                    break;
                }

                case 'l':
                case 'L': {
                    numberOptions.Unlimited = true;
                    if (BigInteger.TryParse(
                        numberOptions.Number.ToString(),
                        out BigInteger result
                    )) {
                        numberOptions.Value = result;
                    }
                    else {
                        unit.Blame(
                            BlameType.InvalidNumberLiteral,
                            tokenStartPosition,
                            Position
                        );
                    }

                    return;
                }

                case 'u':
                case 'U': {
                    numberOptions.Unsigned = true;
                    expectingEndOfNumber   = false;
                    break;
                }

                // Complex numbers
                case 'j':
                case 'J': {
                    numberOptions.Imaginary = true;
                    if (double.TryParse(numberOptions.Number.ToString(), out double imag)) {
                        numberOptions.Value = new Complex(0.0, imag);
                    }
                    else {
                        unit.Blame(
                            BlameType.InvalidComplexNumberLiteral,
                            tokenStartPosition,
                            Position
                        );
                    }

                    break;
                }

                default: {
                    unit.Blame(
                        BlameType.InvalidPostfixInNumberLiteral,
                        tokenStartPosition,
                        Position
                    );
                    return;
                }
            }

            tokenValue.Append(c);
            Move();
        }

        #endregion
    }
}