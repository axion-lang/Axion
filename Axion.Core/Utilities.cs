using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Axion.Core {
    internal static class Utilities {
        private static readonly DateTimeFormatInfo dateTimeFormat =
            new CultureInfo("en-US").DateTimeFormat;

        private const string timedFileNameFormat = "MMM_dd__HH_mm_ss";

        /// <summary>
        ///     Creates a file name from current date and time
        ///     in format: 'yyyy-MMM-dd_HH-mm-ss'.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>file name without extension.</returns>
        internal static string ToFileName(this DateTime dt) {
            return dt.ToString(timedFileNameFormat, dateTimeFormat);
        }

        public static BigInteger RadixLess10ToBigInt(string value, int toRadix) {
            Contract.Assert(toRadix <= 10);
            BigInteger res = 0;
            BigInteger num = BigInteger.Parse(value, NumberStyles.AllowExponent);
            // 1 = (radix^0)
            var radix = 1;
            while (num > 0) {
                // take last digit
                var lastDigit = (int) (num % 10);
                num   /= 10;
                res   += lastDigit * radix;
                radix *= toRadix;
            }

            return res;
        }

        #region Get user input and split it into launch arguments

        /// <summary>
        ///     Splits user command line input to arguments.
        /// </summary>
        /// <returns>Collection of arguments passed into command line.</returns>
        internal static IEnumerable<string> SplitLaunchArguments(string input) {
            var inQuotes = false;
            return Split(
                       input,
                       c => {
                           if (c == '\"') {
                               inQuotes = !inQuotes;
                           }

                           return !inQuotes && char.IsWhiteSpace(c);
                       }
                   )
                   .Select(arg => TrimMatchingChars(arg.Trim(), '\"'))
                   .Where(arg => !string.IsNullOrEmpty(arg));
        }

        private static IEnumerable<string> Split(string str, Func<char, bool> controller) {
            var nextPiece = 0;
            for (var c = 0; c < str.Length; c++) {
                if (controller(str[c])) {
                    yield return str.Substring(nextPiece, c - nextPiece);

                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingChars(string input, char c) {
            if (input.Length >= 2 && input[0] == c && input[input.Length - 1] == c) {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }

        #endregion
    }
}