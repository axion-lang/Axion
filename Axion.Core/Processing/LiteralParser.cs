// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the github.com/IronLanguages/ironpython3/LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Axion.Core.Processing {
    internal static class LiteralParser {
        // ParseComplex helpers
        private static readonly char[] signs = { '+', '-' };

        internal static object ParseInteger(string text, int radix) {
            Debug.Assert(radix != 0);
            if (!ParseInt(text, radix, out int intResult)) {
                BigInteger bigIntResult = ParseBigInteger(text, radix);
                if (!int.TryParse(bigIntResult.ToString(), out intResult)) {
                    return bigIntResult;
                }
            }
            return intResult;
        }

        internal static BigInteger ParseBigInteger(string text, int radix) {
            Debug.Assert(radix != 0);
            BigInteger ret = BigInteger.Zero;
            BigInteger m   = BigInteger.One;
            int        i   = text.Length - 1;
            if (text[i] == 'l' || text[i] == 'L') {
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
                    uval            =  (uint) (CharValue(text[i--], radix) * smallMultiplier + uval);
                    smallMultiplier *= radix;
                }

                // this is more generous than needed
                ret += m * uval;
                if (i >= 0) {
                    m *= smallMultiplier;
                }
            }
            return ret;
        }

        internal static double ParseFloat(string text) {
            try {
                //
                // Strings that end with '\0' is the specific case that CLR libraries allow,
                // however Python doesn't. Since we use CLR floating point number parser,
                // we must check explicitly for the strings that end with '\0'
                //
                if (!string.IsNullOrEmpty(text) && text[text.Length - 1] == '\0') {
                    throw new Exception("null byte in float literal");
                }
                return ParseFloatNoCatch(text);
            }
            catch (OverflowException) {
                return text.TrimStart().StartsWith("-") ? double.NegativeInfinity : double.PositiveInfinity;
            }
        }

        internal static Complex ParseImaginary(string text) {
            try {
                return new Complex(
                    0.0,
                    double.Parse(
                        text.Substring(0, text.Length - 1),
                        CultureInfo.InvariantCulture.NumberFormat
                    )
                );
            }
            catch (OverflowException) {
                return new Complex(0, double.PositiveInfinity);
            }
        }

        internal static bool HexValue(char ch, out int value) {
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

        internal static int HexValue(char ch) {
            if (!HexValue(ch, out int value)) {
                throw new Exception("bad char for integer value: " + ch);
            }
            return value;
        }

        internal static bool TryParseInt(string text, int start, int length, int radix, out int value) {
            value = 0;
            if (start + length > text.Length) {
                return false;
            }
            for (int i = start, end = start + length; i < end; i++) {
                if (HexValue(text[i], out int oneChar) && oneChar < radix) {
                    value = value * radix + oneChar;
                }
                else {
                    return false;
                }
            }
            return true;
        }

        private static object ParseIntegerSign(string text, int radix, int start = 0) {
            int end = text.Length, savedRadix = radix, savedStart = start;
            if (start < 0 || start > end) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            short sign = 1;
            if (radix < 0 || radix == 1 || radix > 36) {
                throw new Exception("base must be >= 2 and <= 36");
            }
            ParseIntegerStart(text, ref radix, ref start, end, ref sign);
            var ret = 0;
            try {
                int saveStart = start;
                for (;;) {
                    if (start >= end) {
                        if (saveStart == start) {
                            throw new Exception($"invalid literal for int() with base {radix}: '{text}'");
                        }
                        break;
                    }
                    if (!HexValue(text[start], out int digit)) {
                        break;
                    }
                    if (!(digit < radix)) {
                        if (text[start] == 'l' || text[start] == 'L') {
                            break;
                        }
                        throw new Exception($"invalid literal for int() with base {radix}: '{text}'");
                    }
                    checked {
                        // include sign here so that System.Int32.MinValue won't overflow
                        ret = ret * radix + sign * digit;
                    }
                    start++;
                }
            }
            catch (OverflowException) {
                return ParseBigIntegerSign(text, savedRadix, savedStart);
            }
            ParseIntegerEnd(text, start, end);
            return ret;
        }

        private static BigInteger ParseBigIntegerSign(string text, int radix, int start = 0) {
            int end = text.Length;
            if (start < 0 || start > end) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            short sign = 1;
            if (radix < 0 || radix == 1 || radix > 36) {
                throw new Exception("base must be >= 2 and <= 36");
            }
            ParseIntegerStart(text, ref radix, ref start, end, ref sign);
            BigInteger ret       = BigInteger.Zero;
            int        saveStart = start;
            for (;;) {
                if (start >= end) {
                    if (start == saveStart) {
                        throw new Exception($"invalid literal for int() with base {radix}: {text}");
                    }
                    break;
                }
                if (!HexValue(text[start], out int digit)) {
                    break;
                }
                if (!(digit < radix)) {
                    if (text[start] == 'l' || text[start] == 'L') {
                        break;
                    }
                    throw new Exception($"invalid literal for int() with base {radix}: {text}");
                }
                ret = ret * radix + digit;
                start++;
            }
            if (start < end && (text[start] == 'l' || text[start] == 'L')) {
                start++;
            }
            ParseIntegerEnd(text, start, end);
            return sign < 0 ? -ret : ret;
        }

        private static Complex ParseComplex(string s) {
            // remove no-meaning spaces and convert to lowercase
            string text = s.Trim().ToUpper();
            if (string.IsNullOrEmpty(text) || text.IndexOf(' ') != -1) {
                throw ExnMalformed();
            }

            // remove 1 layer of parens
            if (text.StartsWith("(") && text.EndsWith(")")) {
                text = text.Substring(1, text.Length - 2);
            }
            try {
                int    len = text.Length;
                string real, imaginary;
                if (text[len - 1] == 'J') {
                    // last sign delimits real and imaginary...
                    int signPos = text.LastIndexOfAny(signs);
                    // ... unless it's after 'e', so we bypass up to 2 of those here
                    for (var i = 0; signPos > 0 && text[signPos - 1] == 'E'; i++) {
                        if (i == 2) {
                            // too many 'e's
                            throw ExnMalformed();
                        }
                        signPos = text.Substring(0, signPos - 1).LastIndexOfAny(signs);
                    }

                    // no real component
                    if (signPos < 0) {
                        return new Complex(0.0, len == 1 ? 1 : ParseFloatNoCatch(text.Substring(0, len - 1)));
                    }
                    real = text.Substring(0,       signPos);
                    imaginary = text.Substring(signPos, len - signPos - 1);
                    if (imaginary.Length == 1) {
                        imaginary += "1"; // convert +/- to +1/-1
                    }
                }
                else {
                    // 'j' delimits real and imaginary
                    string[] splitText = text.Split('J');

                    // no imaginary component
                    if (splitText.Length == 1) {
                        return new Complex(ParseFloatNoCatch(text), 0.0);
                    }

                    // there should only be one j
                    if (splitText.Length != 2) {
                        throw ExnMalformed();
                    }
                    real = splitText[1];
                    imaginary = splitText[0];

                    // a sign must follow the 'j'
                    if (!(real.StartsWith("+") || real.StartsWith("-"))) {
                        throw ExnMalformed();
                    }
                }
                return new Complex(string.IsNullOrEmpty(real) ? 0 : ParseFloatNoCatch(real), ParseFloatNoCatch(imaginary));
            }
            catch (OverflowException) {
                throw new Exception("complex() literal too large to convert");
            }
            catch {
                throw ExnMalformed();
            }
        }

        private static List<byte> ParseBytes(
            string text,
            int    start,
            int    length,
            bool   isRaw,
            bool   normalizeLineEndings
        ) {
            Debug.Assert(text != null);
            var buf = new List<byte>(length);
            int i   = start;
            int l   = start + length;
            while (i < l) {
                char ch = text[i++];
                if (!isRaw && ch == '\\') {
                    if (i >= l) {
                        throw new Exception("Trailing \\ in string");
                    }
                    ch = text[i++];
                    int val;
                    switch (ch) {
                        case 'a': {
                            buf.Add((byte) '\a');
                            continue;
                        }
                        case 'b': {
                            buf.Add((byte) '\b');
                            continue;
                        }
                        case 'f': {
                            buf.Add((byte) '\f');
                            continue;
                        }
                        case 'n': {
                            buf.Add((byte) '\n');
                            continue;
                        }
                        case 'r': {
                            buf.Add((byte) '\r');
                            continue;
                        }
                        case 't': {
                            buf.Add((byte) '\t');
                            continue;
                        }
                        case 'v': {
                            buf.Add((byte) '\v');
                            continue;
                        }
                        case '\\': {
                            buf.Add((byte) '\\');
                            continue;
                        }
                        case '\'': {
                            buf.Add((byte) '\'');
                            continue;
                        }
                        case '\"': {
                            buf.Add((byte) '\"');
                            continue;
                        }
                        case '\r': {
                            if (i < l && text[i] == '\n') {
                                i++;
                            }
                            continue;
                        }
                        case '\n': {
                            continue;
                        }
                        case 'x': //hex
                        {
                            if (!TryParseInt(text, i, 2, 16, out val)) {
                                goto default;
                            }
                            buf.Add((byte) val);
                            i += 2;
                            continue;
                        }
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7': {
                            val = ch - '0';
                            if (i < l && HexValue(text[i], out int onechar) && onechar < 8) {
                                val = val * 8 + onechar;
                                i++;
                                if (i < l && HexValue(text[i], out onechar) && onechar < 8) {
                                    val = val * 8 + onechar;
                                    i++;
                                }
                            }
                        }
                            buf.Add((byte) val);
                            continue;
                        default: {
                            buf.Add((byte) '\\');
                            buf.Add((byte) ch);
                            continue;
                        }
                    }
                }
                if (ch == '\r' && normalizeLineEndings) {
                    // normalize line endings
                    if (i < text.Length && text[i] == '\n') {
                        i++;
                    }
                    buf.Add((byte) '\n');
                }
                else {
                    buf.Add((byte) ch);
                }
            }
            return buf;
        }

        private static int CharValue(char ch, int b) {
            int val = HexValue(ch);
            if (val >= b) {
                throw new Exception($"bad char for the integer value: '{ch}' (base {b})");
            }
            return val;
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

        private static Exception ExnMalformed() {
            return new Exception("complex() arg is a malformed string");
        }
    }
}