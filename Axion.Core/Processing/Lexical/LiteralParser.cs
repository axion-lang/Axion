// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the github.com/IronLanguages/ironpython3/LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using Axion.Core.Processing.Errors;

namespace Axion.Core.Processing.Lexical {
    internal static class LiteralParser {
        internal static object ParseInteger(string input, int radix) {
            Debug.Assert(radix != 0);

            if (!ParseInt(input, radix, out int intResult)) {
                BigInteger bigIntResult = ParseBigInteger(input, radix);
                if (!int.TryParse(bigIntResult.ToString(), out intResult)) {
                    return bigIntResult;
                }
            }
            return intResult;
        }

        internal static BigInteger ParseBigInteger(string input, int radix) {
            Debug.Assert(radix != 0);

            BigInteger result = BigInteger.Zero;
            BigInteger m      = BigInteger.One;
            int        i      = input.Length - 1;
            if (input[i] == 'l' || input[i] == 'L') {
                i--;
            }
            var groupMax = 7;
            if (radix <= 10) {
                groupMax = 9; // 2 147 483 647
            }
            while (i >= 0) {
                // extract digits in a batch
                var  smallMultiplier = 1;
                uint uval            = 0;
                for (var j = 0; j < groupMax && i >= 0; j++) {
                    uval            =  (uint) (CharValue(input[i--], radix) * smallMultiplier + uval);
                    smallMultiplier *= radix;
                }

                // this is more generous than needed
                result += m * uval;
                if (i >= 0) {
                    m *= smallMultiplier;
                }
            }
            return result;
        }

        /// <summary>
        ///     Gets floating number (string) as input,
        ///     returns double value.
        /// </summary>
        internal static double ParseFloat(string input) {
            try {
                return ParseFloatNoCatch(input);
            }
            catch (OverflowException) {
                return input.TrimStart().StartsWith("-")
                           ? double.NegativeInfinity
                           : double.PositiveInfinity;
            }
        }

        internal static bool TryParseInt(string input, int radix, out int value) {
            value = 0;
            for (var i = 0; i < input.Length; i++) {
                if (HexValue(input[i], out int oneChar) && oneChar < radix) {
                    value = value * radix + oneChar;
                }
                else {
                    return false;
                }
            }
            return true;
        }

        private static object ParseIntegerSign(string input, int radix, int start = 0) {
            int end = input.Length, savedRadix = radix, savedStart = start;
            if (start < 0 || start > end) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            short sign = 1;
            if (radix <= 1 || radix > 36) {
                throw new Exception("base must be >= 2 and <= 36");
            }
            ParseIntegerStart(input, ref radix, ref start, end, ref sign);
            var ret = 0;
            try {
                int saveStart = start;
                while (true) {
                    if (start >= end) {
                        if (saveStart == start) {
                            throw new Exception($"invalid literal for int() with base {radix}: '{input}'");
                        }
                        break;
                    }
                    if (!HexValue(input[start], out int digit)) {
                        break;
                    }
                    if (digit >= radix) {
                        if (input[start] == 'l' || input[start] == 'L') {
                            break;
                        }
                        throw new Exception($"invalid literal for int() with base {radix}: '{input}'");
                    }
                    checked {
                        // include sign here so that System.Int32.MinValue won't overflow
                        ret = ret * radix + sign * digit;
                    }
                    start++;
                }
            }
            catch (OverflowException) {
                return ParseBigIntegerSign(input, savedRadix, savedStart);
            }
            ParseIntegerEnd(input, start, end);
            return ret;
        }

        private static BigInteger ParseBigIntegerSign(string input, int radix, int start = 0) {
            int end = input.Length;
            if (start < 0 || start > end) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            short sign = 1;
            if (radix < 0 || radix == 1 || radix > 36) {
                throw new Exception("base must be >= 2 and <= 36");
            }
            ParseIntegerStart(input, ref radix, ref start, end, ref sign);
            BigInteger ret       = BigInteger.Zero;
            int        saveStart = start;
            while (true) {
                if (start >= end) {
                    if (start == saveStart) {
                        throw new Exception($"invalid literal for int() with base {radix}: {input}");
                    }
                    break;
                }
                if (!HexValue(input[start], out int digit)) {
                    break;
                }
                if (digit >= radix) {
                    if (input[start] == 'l' || input[start] == 'L') {
                        break;
                    }
                    throw new Exception($"invalid literal for int() with base {radix}: {input}");
                }
                ret = ret * radix + digit;
                start++;
            }
            if (start < end && (input[start] == 'l' || input[start] == 'L')) {
                start++;
            }
            ParseIntegerEnd(input, start, end);
            return sign < 0 ? -ret : ret;
        }

        /// <summary>
        ///     Gets imaginary (j) number as input,
        ///     returns <see cref="Complex" /> number.
        /// </summary>
        internal static Complex ParseImaginary(string input) {
            try {
                return new Complex(
                    0.0,
                    double.Parse(
                        // remove 'j' from end
                        input.Substring(0, input.Length - 1),
                        CultureInfo.InvariantCulture.NumberFormat
                    )
                );
            }
            catch (OverflowException) {
                return new Complex(0, double.PositiveInfinity);
            }
        }

        private static bool ParseInt(string text, int radix, out int ret) {
            ret = 0;
            long m = 1;
            for (int i = text.Length - 1; i >= 0; i--) {
                // avoid the exception here. Not only is throwing it expensive,
                // but loading the resources for it is also expensive 
                long lret = ret + m * CharValue(text[i], radix);
                if (int.MinValue <= lret && lret <= int.MaxValue) {
                    ret = (int) lret;
                }
                else {
                    return false;
                }
                m *= radix;
                if (int.MinValue > m || m > int.MaxValue) {
                    return false;
                }
            }
            return true;
        }

        private static void ParseIntegerStart(string text, ref int radix, ref int start, int end, ref short sign) {
            //  Skip whitespace
            while (start < end && char.IsWhiteSpace(text, start)) {
                start++;
            }
            //  Sign?
            if (start < end) {
                switch (text[start]) {
                    case '-': {
                        sign = -1;
                        goto case '+';
                    }
                    case '+': {
                        start++;
                        break;
                    }
                }
            }
            //  Skip whitespace
            while (start < end && char.IsWhiteSpace(text, start)) {
                start++;
            }

            //  Determine base
            if (radix == 0) {
                if (start < end && text[start] == '0') {
                    // Hex, oct, or bin
                    if (++start < end) {
                        switch (text[start]) {
                            case 'x':
                            case 'X': {
                                start++;
                                radix = 16;
                                break;
                            }
                            case 'o':
                            case 'O': {
                                radix = 8;
                                start++;
                                break;
                            }
                            case 'b':
                            case 'B': {
                                start++;
                                radix = 2;
                                break;
                            }
                        }
                    }
                    if (radix == 0) {
                        // Keep the leading zero
                        start--;
                        radix = 8;
                    }
                }
                else {
                    radix = 10;
                }
            }
        }

        private static void ParseIntegerEnd(string text, int start, int end) {
            //  Skip whitespace
            while (start < end && char.IsWhiteSpace(text, start)) {
                start++;
            }
            if (start < end) {
                throw new Exception("invalid integer number literal");
            }
        }

        private static double ParseFloatNoCatch(string text) {
            string s = ReplaceUnicodeDigits(text);
            switch (s.ToUpper(CultureInfo.InvariantCulture).TrimStart()) {
                case "NAN":
                case "+NAN":
                case "-NAN": {
                    return double.NaN;
                }
                case "INF":
                case "+INF": {
                    return double.PositiveInfinity;
                }
                case "-INF": {
                    return double.NegativeInfinity;
                }
                default: {
                    // pass NumberStyles to disallow ,'s in float strings.
                    double res = double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
                    return res == 0.0 && text.TrimStart().StartsWith("-") ? 0.0 : res;
                }
            }
        }

        private static string ReplaceUnicodeDigits(string text) {
            StringBuilder replacement = null;
            for (var i = 0; i < text.Length; i++) {
                if (text[i] >= '\x660' && text[i] <= '\x669') {
                    if (replacement == null) {
                        replacement = new StringBuilder(text);
                    }
                    replacement[i] = (char) (text[i] - '\x660' + '0');
                }
            }
            if (replacement != null) {
                text = replacement.ToString();
            }
            return text;
        }

        private static int CharValue(char ch, int b) {
            HexValue(ch, out int val, out ErrorType _);
            if (val >= b) {
                throw new Exception($"bad char for the integer value: '{ch}' (base {b})");
            }
            return val;
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

        internal static void HexValue(char ch, out int result, out ErrorType errorType) {
            errorType = ErrorType.None;
            if (!HexValue(ch, out result)) {
                errorType = ErrorType.BadCharacterForIntegerValue;
            }
        }
    }
}