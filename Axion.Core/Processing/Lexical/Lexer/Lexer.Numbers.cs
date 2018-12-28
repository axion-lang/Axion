using System.Linq;
using System.Numerics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
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
                    Blame(BlameType.ExpectedNumberValueAfterNumberBaseSpecifier, tokenStartPosition, Stream.Position);
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
                numberOptions.Number.Append("0");
            }
            // c is digit or dot except 0 here
            while (Spec.IsLetterOrNumberPart(c)) {
                if (char.IsDigit(c)) {
                    numberOptions.Number.Append(c);
                }
                else if (c != '_') {
                    if (c == '.') {
                        Position dotPosition = Stream.Position;
                        if (!char.IsDigit(Stream.Peek)) {
                            // found non-digit after dot: '.' is operator on some number.
                            // leaving '.' to next token.
                            break;
                        }
                        // if found second dot in number
                        if (numberOptions.Floating) {
                            tokenValue.Append(c);
                            Stream.Move();
                            Blame(BlameType.RepeatedDotInNumberLiteral, dotPosition, Stream.Position);
                        }
                        else {
                            numberOptions.Number.Append(c);
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
                        Blame(BlameType.InvalidNumberLiteral, tokenStartPosition, Stream.Position);
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
                        if (longValue != 0L) {
                            // ignore leading 0's...
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
                            ReadNumberPostfix(
                                numberOptions,
                                false
                            );
                            return numberOptions;
                        }
                        // invalid
                        Blame(BlameType.InvalidBinaryLiteral, tokenStartPosition, Stream.Position);
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
                    numberOptions.Number.Append(c);
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
                    Blame(BlameType.InvalidOctalLiteral, tokenStartPosition, Stream.Position);
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
                    numberOptions.Number.Append(c);
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
                    Blame(BlameType.InvalidHexadecimalLiteral, tokenStartPosition, Stream.Position);
                }
                tokenValue.Append(c);
                Stream.Move();
                isOnBaseLetter = false;
            }
            return numberOptions;
        }

        private void ReadExponent(
            in NumberOptions numberOptions
        ) {
            Position ePosition = Stream.Position;
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
                Blame(BlameType.RedundantExponentForZeroNumber, ePosition, Stream.Position);
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
                    Blame(BlameType.ExpectedNumberAfterExponentSign, ePosition, Stream.Position);
                    break;
                }
                tokenValue.Append(c);
                Stream.Move();
            }

            if (!hasValue) {
                Blame(BlameType.ExpectedNumberAfterExponentSign, ePosition, Stream.Position);
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
            in NumberOptions numberOptions,
            bool             isOnBaseLetter
        ) {
            Position postfixPosition = Stream.Position;
            // c is letter here
            if (isOnBaseLetter) {
                Blame(BlameType.ExpectedNumberValueAfterNumberBaseSpecifier, tokenStartPosition, Stream.Position);
            }

            ReadNumberPostfixLetters(numberOptions, out bool expectingEndOfNumber, out bool bitRateRequired);

            if (Spec.IsLetterOrNumberPart(c) && expectingEndOfNumber) {
                Blame(BlameType.ExpectedEndOfNumberAfterPostfix, postfixPosition, Stream.Position);
            }
            else if (char.IsDigit(c)) {
                Position bitRatePosition = Stream.Position;
                // Reading number bit rate
                var bitRateStr = "";
                while (char.IsDigit(c)) {
                    tokenValue.Append(c);
                    bitRateStr += c;
                    Stream.Move();
                }

                numberOptions.Bits = int.Parse(bitRateStr);

                // check for invalid bit rates
                if (numberOptions.Floating) {
                    if (!Spec.FloatBitRates.Contains(numberOptions.Bits)) {
                        Blame(BlameType.InvalidFloatNumberBitRate, bitRatePosition, Stream.Position);
                    }
                }
                else if (!Spec.IntegerBitRates.Contains(numberOptions.Bits)) {
                    Blame(BlameType.InvalidIntegerNumberBitRate, bitRatePosition, Stream.Position);
                }

                if (char.IsLetter(c) || c == '_') {
                    // number can't be followed by these characters
                    Blame(BlameType.ExpectedEndOfNumberAfterPostfix, postfixPosition, Stream.Position);
                }
            }
            else if (bitRateRequired) {
                // expected digit after num 'i#' postfix
                Blame(BlameType.ExpectedABitRateAfterNumberPostfix, postfixPosition, Stream.Position);
            }
        }

        private void ReadNumberPostfixLetters(in NumberOptions numberOptions, out bool expectingEndOfNumber, out bool bitRateRequired) {
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
                    if (BigInteger.TryParse(numberOptions.Number.ToString(), out BigInteger result)) {
                        numberOptions.Value = result;
                    }
                    else {
                        Blame(BlameType.InvalidNumberLiteral, tokenStartPosition, Stream.Position);
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
                        Blame(BlameType.InvalidComplexNumberLiteral, tokenStartPosition, Stream.Position);
                    }
                    break;
                }
                default: {
                    Blame(BlameType.InvalidPostfixInNumberLiteral, tokenStartPosition, Stream.Position);
                    return;
                }
            }
            tokenValue.Append(c);
            Stream.Move();
        }
    }
}