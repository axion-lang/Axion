using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    public partial class Lexer {
        private NumberToken ReadNumber() {
            var           errors   = new List<ErrorType>();
            var           warnings = new List<WarningType>();
            NumberOptions numberOptions;
            if (c == '0') {
                tokenValue.Append("0");
                Stream.Move();

                bool isOnBaseLetter;
                // on second char (base letter, determines radix)
                if (c == 'b' || c == 'B') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions = ReadBinaryNumber(out isOnBaseLetter, errors);
                }
                else if (c == 'o' || c == 'O') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions = ReadOctalNumber(out isOnBaseLetter, errors);
                }
                else if (c == 'x' || c == 'X') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions = ReadHexNumber(out isOnBaseLetter, errors);
                }
                else {
                    // regular num with 0's at beginning
                    isOnBaseLetter = false;
                    // skip leading zeros
                    while (c == '0') {
                        tokenValue.Append("0");
                        Stream.Move();
                    }
                    numberOptions = ReadDecimalNumber(errors, warnings);
                }

                // '0x', '0b', '0o'
                if (isOnBaseLetter) {
                    errors.Add(ErrorType.ExpectedDigitAfterNumberBaseSpecifier);
                }
            }
            else {
                // c in (1..9)
                numberOptions = ReadDecimalNumber(errors, warnings);
            }

            var number = new NumberToken(tokenStartPosition, tokenValue, numberOptions);
            if (warnings.Count > 0) {
                foreach (WarningType warning in warnings) {
                    ReportWarning(
                        warning,
                        number
                    );
                }
            }
            if (errors.Count > 0) {
                foreach (ErrorType error in errors) {
                    ReportError(
                        error,
                        number
                    );
                }
            }
            return number;
        }

        private NumberOptions ReadDecimalNumber(
            List<ErrorType>   errors,
            List<WarningType> warnings
        ) {
            var numberOptions = new NumberOptions { Radix = 10 };
            // c is digit or dot except 0 here
            var isOnBaseLetter = true;
            while (Spec.IsLetterOrNumberPart(c)) {
                if (!char.IsDigit(c) && c != '_') {
                    if (c == '.') {
                        if (!char.IsDigit(Stream.Peek) || Stream.Peek == '.') {
                            // found .. in number (probably range operator)
                            // or found non-digit after dot: '.' is operator on some number.
                            // leaving '.' or '..' to next token.
                            break;
                        }
                        // if found second dot in number
                        if (numberOptions.Floating) {
                            tokenValue.Append(c);
                            Stream.Move();
                            errors.Add(ErrorType.RepeatedDotInNumberLiteral);
                        }
                        numberOptions.Floating = true;
                    }
                    else if (c == 'e' || c == 'E') {
                        ReadExponent(numberOptions, errors, warnings);
                        continue;
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter,
                            errors
                        );
                        return numberOptions;
                    }
                    else {
                        // invalid
                        errors.Add(ErrorType.InvalidIntegerLiteral);
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private NumberOptions ReadBinaryNumber(
            out bool          isOnBaseLetter,
            List<ErrorType>   errors
        ) {
            var        numberOptions = new NumberOptions { Radix = 2 };
            var        bitsCount     = 0;
            var        longValue     = 0L;
            BigInteger bigInt        = BigInteger.Zero;
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
                                errors
                            );
                            return numberOptions;
                        }
                        // invalid
                        errors.Add(ErrorType.InvalidBinaryLiteral);
                        break;
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private NumberOptions ReadOctalNumber(
            out bool          isOnBaseLetter,
            List<ErrorType>   errors
        ) {
            var numberOptions = new NumberOptions { Radix = 8 };
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (!c.IsValidOctalDigit() && c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter,
                            errors
                        );
                        return numberOptions;
                    }
                    // invalid
                    errors.Add(ErrorType.InvalidOctalLiteral);
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private NumberOptions ReadHexNumber(
            out bool          isOnBaseLetter,
            List<ErrorType>   errors
        ) {
            var numberOptions = new NumberOptions { Radix = 16 };
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (!c.IsValidHexadecimalDigit() && c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter,
                            errors
                        );
                        return numberOptions;
                    }
                    // invalid
                    errors.Add(ErrorType.InvalidHexadecimalLiteral);
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private void ReadExponent(
            NumberOptions     numberOptions,
            List<ErrorType>   errors,
            List<WarningType> warnings
        ) {
            // check for '0'
            string num = tokenValue.ToString().Replace("_", "").Trim('0');
            if (tokenValue.Length > 0
             && (num == "" || num == ".")) {
                warnings.Add(WarningType.RedundantExponentForZeroNumber);
            }
            
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
                    errors.Add(ErrorType.InvalidValueAfterExponent);
                    break;
                }
                tokenValue.Append(c);
                Stream.Move();
            }

            if (!hasValue) {
                errors.Add(ErrorType.ExpectedDigitAfterNumberExponent);
            }
            else {
                numberOptions.Exponent = int.Parse(eValue);
            }

            if (hasPostfix) {
                ReadNumberPostfix(
                    numberOptions,
                    false,
                    errors
                );
            }
        }

        private void ReadNumberPostfix(
            NumberOptions     numberOptions,
            bool              isOnBaseLetter,
            List<ErrorType>   errors
        ) {
            // c is letter here
            if (isOnBaseLetter) {
                errors.Add(ErrorType.ExpectedDigitAfterNumberBaseSpecifier);
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
                        errors.Add(ErrorType.InvalidPostfixInNumberLiteral);
                        return;
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
            }

            if (Spec.IsLetterOrNumberPart(c) && expectingEndOfNumber) {
                errors.Add(ErrorType.ExpectedEndOfNumberAfterPostfix);
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
                    errors.Add(ErrorType.InvalidFloatNumberBitRate);
                }
                if (!Spec.IntegerBitRates.Contains(numberOptions.Bits)) {
                    errors.Add(ErrorType.InvalidIntegerNumberBitRate);
                }

                if (char.IsLetter(c) || c == '_') {
                    // number can't be followed by these characters
                    errors.Add(ErrorType.ExpectedEndOfNumberAfterPostfix);
                }
            }
            else if (bitRateRequired) {
                // expected digit after num 'i#' postfix
                errors.Add(ErrorType.ExpectedABitRateAfterNumberPostfix);
            }
        }
    }
}