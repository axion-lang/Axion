using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axion.Core {
    internal static class Utilities {
        private static readonly DateTimeFormatInfo dateTimeFormat      = new CultureInfo("en-US").DateTimeFormat;
        private const           string             timedFileNameFormat = "yyyy-MMM-dd_HH-mm-ss";

        /// <summary>
        ///     Returns count of bits set to 1 on specified <see cref="number" />.
        /// </summary>
        public static int GetSetBitCount(long number) {
            var count = 0;

            // Loop the value while there are still bits
            while (number != 0) {
                // Remove the end bit
                number = number & (number - 1);

                // Increment the count
                count++;
            }

            // Return the count
            return count;
        }

        /// <summary>
        ///     Creates a file name from current date and time
        ///     in format: 'yyyy-MMM-dd_HH-mm-ss'.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>file name without extension.</returns>
        internal static string ToFileName(this DateTime dt) {
            return dt.ToString(timedFileNameFormat, dateTimeFormat);
        }

        /// <summary>
        ///     Returns
        /// </summary>
        internal static string[] GetLines(this string text, int fromIndex, int count) {
            //int newlineIndex = text.IndexOf("\n", StringComparison.InvariantCulture);
            //if (newlineIndex == -1) {
            //    return new[] { text };
            //}
            //var lines = new List<string>();
            //if (fromIndex == 0) {
            //    lines.Add(text.Substring(0, newlineIndex + 1));
            //}
            //int i = 0;
            //int prevNewlineIndex = newlineIndex;
            //while (newlineIndex != -1 && lines.Count < count) {
            //    text = text.Substring(prevNewlineIndex, newlineIndex + 1);
            //    newlineIndex = text.IndexOf("\n", StringComparison.InvariantCulture);
            //    if (i >= fromIndex) {
            //        lines.Add(text.Substring(0, newlineIndex + 1));
            //    }
            //    prevNewlineIndex = newlineIndex;
            //    i++;
            //}
            string[] lines = text.Split(Spec.Newlines, StringSplitOptions.None);
            return lines.Skip(fromIndex).Take(count).ToArray();
        }

        #region Get user input and split it into launch arguments

        /// <summary>
        ///     Splits user command line input to arguments.
        /// </summary>
        /// <returns>Collection of arguments passed into command line.</returns>
        internal static IEnumerable<string> SplitLaunchArguments(string input) {
            var inQuotes = false;
            return Split(
                       input, c => {
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