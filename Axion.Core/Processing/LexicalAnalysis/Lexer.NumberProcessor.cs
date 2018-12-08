using System.Linq;
using System.Numerics;
using Axion.Core.Processing.Errors;
using Axion.Core.Tokens;

namespace Axion.Core.Processing.LexicalAnalysis {
    public partial class Lexer {
        private NumberToken ReadNumber() {
            NumberOptions numberOptions;
            if (c == '0') {
                tokenValue.Append("0");
                Stream.Move();

                bool isOnBaseLetter;
                // on second char (base letter, determines radix)
                if (c == 'b' || c == 'B') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions = ReadBinaryNumber(out isOnBaseLetter);
                }
                else if (c == 'o' || c == 'O') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions = ReadOctalNumber(out isOnBaseLetter);
                }
                else if (c == 'x' || c == 'X') {
                    tokenValue.Append(c);
                    Stream.Move();
                    numberOptions = ReadHexNumber(out isOnBaseLetter);
                }
                else {
                    // regular num with 0's at beginning
                    isOnBaseLetter = false;
                    // skip leading zeros
                    while (c == '0') {
                        tokenValue.Append("0");
                        Stream.Move();
                    }
                    numberOptions = ReadDecimalNumber(true);
                }

                // '0x', '0b', '0o'
                if (isOnBaseLetter) {
                    ReportError(ErrorType.ExpectedNumberValueAfterNumberBaseSpecifier, tokenStartPosition, Stream.Position);
                }
            }
            else {
                // c in (1..9)
                numberOptions = ReadDecimalNumber(false);
            }

            return new NumberToken(tokenStartPosition, tokenValue, numberOptions);
        }

        private NumberOptions ReadDecimalNumber(bool startsWithZero) {
            var numberOptions = new NumberOptions { Radix = 10 };
            if (startsWithZero) {
                numberOptions.Number += "0";
            }
            // c is digit or dot except 0 here
            while (Spec.IsLetterOrNumberPart(c)) {
                if (char.IsDigit(c)) {
                    numberOptions.Number += c;
                }
                else if (c != '_') {
                    if (c == '.') {
                        (int line, int column) dotPosition = Stream.Position;
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
                            ReportError(ErrorType.RepeatedDotInNumberLiteral, dotPosition, Stream.Position);
                        }
                        numberOptions.Floating = true;
                    }
                    else if (c == 'e' || c == 'E') {
                        ReadExponent(numberOptions);
                        continue;
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            false
                        );
                        return numberOptions;
                    }
                    else {
                        // invalid
                        ReportError(ErrorType.InvalidNumberLiteral, tokenStartPosition, Stream.Position);
                    }
                }
                tokenValue.Append(c);
                Stream.Move();
            }
            return numberOptions;
        }

        private NumberOptions ReadBinaryNumber(
            out bool isOnBaseLetter
        ) {
            var        numberOptions = new NumberOptions { Radix = 2 };
            var        bitsCount     = 0;
            var        longValue     = 0L;
            BigInteger bigInt        = BigInteger.Zero;
            isOnBaseLetter = true;
            while (Spec.IsLetterOrNumberPart(c)) {
                switch (c) {
                    case '0': {
                        numberOptions.Number += c;
                        if (longValue != 0L) {
                            // ignore leading 0's...
                            goto case '1';
                        }
                        break;
                    }
                    case '1': {
                        numberOptions.Number += c;
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
                                false
                            );
                            return numberOptions;
                        }
                        // invalid
                        ReportError(ErrorType.InvalidBinaryLiteral, tokenStartPosition, Stream.Position);
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
            out bool isOnBaseLetter
        ) {
            var numberOptions = new NumberOptions { Radix = 8 };
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (c.IsValidOctalDigit()) {
                    numberOptions.Number += c;
                }
                else if (c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter
                        );
                        break;
                    }
                    // invalid
                    ReportError(ErrorType.InvalidOctalLiteral, tokenStartPosition, Stream.Position);
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private NumberOptions ReadHexNumber(
            out bool isOnBaseLetter
        ) {
            var numberOptions = new NumberOptions { Radix = 16 };
            isOnBaseLetter = true;

            while (Spec.IsLetterOrNumberPart(c)) {
                if (c.IsValidHexadecimalDigit()) {
                    numberOptions.Number += c;
                }
                else if (c != '_') {
                    if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            numberOptions,
                            isOnBaseLetter
                        );
                        break;
                    }
                    // invalid
                    ReportError(ErrorType.InvalidHexadecimalLiteral, tokenStartPosition, Stream.Position);
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private void ReadExponent(
            NumberOptions numberOptions
        ) {
            (int line, int column) ePosition = Stream.Position;
            // c == 'e'
            // check for '0'
            string num = tokenValue.ToString().Replace("_", "").Trim('0');
            bool zero = tokenValue.Length > 0
                     && (num == "" || num == ".");

            numberOptions.HasExponent = true;
            var hasValue   = false;
            var hasPostfix = false;
            tokenValue.Append(c);
            Stream.Move();

            if (zero) {
                ReportWarning(WarningType.RedundantExponentForZeroNumber, ePosition, Stream.Position);
            }

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
                    ReportError(ErrorType.ExpectedNumberAfterExponentSign, ePosition, Stream.Position);
                    break;
                }
                tokenValue.Append(c);
                Stream.Move();
            }

            if (!hasValue) {
                ReportError(ErrorType.ExpectedNumberAfterExponentSign, ePosition, Stream.Position);
            }
            else {
                numberOptions.Exponent = int.Parse(eValue);
            }

            if (hasPostfix) {
                ReadNumberPostfix(
                    numberOptions,
                    false
                );
            }
        }

        private void ReadNumberPostfix(
            NumberOptions numberOptions,
            bool          isOnBaseLetter
        ) {
            (int line, int column) postfixPosition = Stream.Position;
            // c is letter here
            if (isOnBaseLetter) {
                ReportError(ErrorType.ExpectedNumberValueAfterNumberBaseSpecifier, tokenStartPosition, Stream.Position);
            }

            // add postfix letters
            var expectingEndOfNumber = true;
            var bitRateRequired      = false;
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
                    if (BigInteger.TryParse(numberOptions.Number, out BigInteger result)) {
                        numberOptions.Value = result;
                    }
                    else {
                        ReportError(ErrorType.InvalidNumberLiteral, tokenStartPosition, Stream.Position);
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
                    if (double.TryParse(numberOptions.Number, out double imag)) {
                        numberOptions.Value = new Complex(0.0, imag);
                    }
                    else {
                        ReportError(ErrorType.InvalidComplexNumberLiteral, tokenStartPosition, Stream.Position);
                    }
                    break;
                }
                default: {
                    ReportError(ErrorType.InvalidPostfixInNumberLiteral, tokenStartPosition, Stream.Position);
                    return;
                }
            }
            tokenValue.Append(c);
            Stream.Move();

            if (Spec.IsLetterOrNumberPart(c) && expectingEndOfNumber) {
                ReportError(ErrorType.ExpectedEndOfNumberAfterPostfix, postfixPosition, Stream.Position);
            }
            else if (char.IsDigit(c)) {
                (int line, int column) bitRatePosition = Stream.Position;
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
                    ReportError(ErrorType.InvalidFloatNumberBitRate, bitRatePosition, Stream.Position);
                }
                if (!Spec.IntegerBitRates.Contains(numberOptions.Bits)) {
                    ReportError(ErrorType.InvalidIntegerNumberBitRate, bitRatePosition, Stream.Position);
                }

                if (char.IsLetter(c) || c == '_') {
                    // number can't be followed by these characters
                    ReportError(ErrorType.ExpectedEndOfNumberAfterPostfix, postfixPosition, Stream.Position);
                }
            }
            else if (bitRateRequired) {
                // expected digit after num 'i#' postfix
                ReportError(ErrorType.ExpectedABitRateAfterNumberPostfix, postfixPosition, Stream.Position);
            }
        }
    }
}