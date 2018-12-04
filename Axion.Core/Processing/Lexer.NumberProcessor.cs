using System.Linq;
using System.Numerics;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    public partial class Lexer {
        private NumberToken ReadNumber() {
            var errorType     = ErrorType.None;
            var numberOptions = new NumberOptions();
            if (c == '0') {
                tokenValue.Append("0");
                Stream.Move();

                bool isOnBaseLetter;
                // second char (determines radix)
                if (c == 'b' || c == 'B') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions.Radix = 2;
                    ReadBinaryNumber(numberOptions, out isOnBaseLetter, ref errorType);
                }
                else if (c == 'o' || c == 'O') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions.Radix = 8;
                    ReadOctalNumber(numberOptions, out isOnBaseLetter, ref errorType);
                }
                else if (c == 'x' || c == 'X') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions.Radix = 16;
                    ReadHexNumber(numberOptions, out isOnBaseLetter, ref errorType);
                }
                else {
                    isOnBaseLetter = false;
                    // skip leading zeros
                    while (c == '0') {
                        tokenValue.Append("0");
                        Stream.Move();
                    }
                    ReadDecimalNumber(numberOptions, ref errorType);
                }

                if (isOnBaseLetter) {
                    errorType = ErrorType.ExpectedDigitAfterNumberBaseSpecifier;
                }
            }
            else {
                ReadDecimalNumber(numberOptions, ref errorType);
            }

            if (errorType == ErrorType.None) {
                return new NumberToken(tokenStartPosition, tokenValue, numberOptions);
            }

            var invalidNumber = new NumberToken(tokenStartPosition, tokenValue, numberOptions);
            ReportError(
                errorType,
                invalidNumber
            );
            return invalidNumber;
        }

        private void ReadDecimalNumber(
            NumberOptions numberOptions,
            ref ErrorType errorType
        ) {
            // c is digit or dot except 0 here
            var isOnBaseLetter = true;
            while (Spec.IsLetterOrNumberPart(c)) {
                if (!char.IsDigit(c) && c != '_') {
                    if (c == '.') {
                        // BUG: no error after dot
                        if (char.IsDigit(Stream.Peek())) {
                            // if found second dot
                            if (numberOptions.Floating) {
                                tokenValue.Append(c);
                                Stream.Move();
                                errorType = ErrorType.RepeatedDotInNumberLiteral;
                            }
                            numberOptions.Floating = true;
                        }
                        // non-digit after dot: '.' is operator on some number
                        // leave dot to next token.
                        else {
                            break;
                        }
                    }
                    else if (c == 'e' || c == 'E') {
                        ReadExponent(numberOptions, ref errorType);
                        continue;
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter,
                            ref errorType
                        );
                        break;
                    }
                    else {
                        // invalid
                        errorType = ErrorType.InvalidIntegerLiteral;
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
        }

        private void ReadBinaryNumber(
            NumberOptions numberOptions,
            out bool      isOnBaseLetter,
            ref ErrorType errorType
        ) {
            var        bitsCount = 0;
            var        longValue = 0L;
            BigInteger bigInt    = BigInteger.Zero;
            isOnBaseLetter = true;
            while (Spec.IsLetterOrNumberPart(c)) {
                switch (c) {
                    case '0': {
                        if (longValue != 0L) {
                            // ignore leading 0's...
                            goto case '1';
                        }
                        break;
                    }
                    case '1': {
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
                            ReadNumberPostfix(
                                numberOptions,
                                false,
                                ref errorType
                            );
                            return;
                        }
                        // invalid
                        errorType = ErrorType.InvalidBinaryLiteral;
                        break;
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
        }

        private void ReadOctalNumber(
            NumberOptions numberOptions,
            out bool      isOnBaseLetter,
            ref ErrorType errorType
        ) {
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (!c.IsValidOctalDigit() && c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter,
                            ref errorType
                        );
                        return;
                    }
                    // invalid
                    errorType = ErrorType.InvalidOctalLiteral;
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
        }

        private void ReadHexNumber(
            NumberOptions numberOptions,
            out bool      isOnBaseLetter,
            ref ErrorType errorType
        ) {
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (!c.IsValidHexadecimalDigit() && c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter,
                            ref errorType
                        );
                        return;
                    }
                    // invalid
                    errorType = ErrorType.InvalidHexadecimalLiteral;
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
        }

        private void ReadExponent(
            NumberOptions numberOptions,
            ref ErrorType errorType
        ) {
            numberOptions.HasExponent = true;
            // c == 'e'
            var hasValue   = false;
            var hasPostfix = false;
            tokenValue.Append(c);
            Stream.Move();

            var eValue = "";
            if (c == '-' || c == '+') {
                if (c == '-') {
                    eValue += c;
                }
                tokenValue.Append(c);
                Stream.Move();
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
                    errorType = ErrorType.InvalidValueAfterExponent;
                    break;
                }
                tokenValue.Append(c);
                Stream.Move();
            }

            if (!hasValue) {
                errorType = ErrorType.ExpectedDigitAfterNumberExponent;
            }
            else {
                numberOptions.Exponent = int.Parse(eValue);
            }

            if (hasPostfix) {
                ReadNumberPostfix(
                    numberOptions,
                    false,
                    ref errorType
                );
            }
        }

        private void ReadNumberPostfix(
            NumberOptions numberOptions,
            bool          isOnBaseLetter,
            ref ErrorType errorType
        ) {
            // letter here
            if (isOnBaseLetter) {
                errorType = ErrorType.ExpectedDigitAfterNumberBaseSpecifier;
            }

            // add postfix letters
            var expectingEndOfNumber = true;
            var bitRateRequired      = false;
            if (char.IsLetter(c)) {
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
                        break;
                    }
                    default: {
                        errorType = ErrorType.InvalidPostfixInNumberLiteral;
                        return;
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
            }

            if (Spec.IsLetterOrNumberPart(c) && expectingEndOfNumber) {
                errorType = ErrorType.ExpectedEndOfNumberAfterPostfix;
            }
            else if (char.IsDigit(c)) {
                // Reading number bit rate
                var bitRateStr = "";
                while (char.IsDigit(c)) {
                    tokenValue.Append(c);
                    bitRateStr += c;
                    Stream.Move();
                }

                numberOptions.Bits = int.Parse(bitRateStr);

                // check for invalid bit rates
                if (numberOptions.Floating &&
                    !Spec.FloatBitRates.Contains(numberOptions.Bits)) {
                    errorType = ErrorType.InvalidFloatNumberBitRate;
                }
                if (!Spec.IntegerBitRates.Contains(numberOptions.Bits)) {
                    errorType = ErrorType.InvalidIntegerNumberBitRate;
                }

                if (char.IsLetter(c) || c == '_') {
                    // number can't be followed by these characters
                    errorType = ErrorType.ExpectedEndOfNumberAfterPostfix;
                }
            }
            else if (bitRateRequired) {
                // expected digit after num 'i#' postfix
                errorType = ErrorType.ExpectedABitRateAfterNumberPostfix;
            }
        }
    }
}