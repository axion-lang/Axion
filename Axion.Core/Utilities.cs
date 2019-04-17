﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Axion.Core.Specification;

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

        internal static string GetValue(this TokenType type) {
            string value = Spec.Keywords.FirstOrDefault(kvp => kvp.Value == type).Key;
            if (value != null) {
                return value;
            }

            value = Spec.Operators.FirstOrDefault(kvp => kvp.Value.Type == type).Key;
            if (value != null) {
                return value;
            }

            value = Spec.Symbols.FirstOrDefault(kvp => kvp.Value == type).Key;
            if (value != null) {
                return value;
            }

            return type.ToString("G");
        }

        internal static string GetExprFriendlyName(Type expressionType) {
            string exprOriginalName = expressionType.Name.Replace("Expression", "");
            var    result           = new StringBuilder();
            result.Append("'" + char.ToLower(exprOriginalName[0]));

            exprOriginalName = exprOriginalName.Remove(0, 1);
            foreach (char c in exprOriginalName) {
                if (char.IsUpper(c)) {
                    result.Append(" ").Append(char.ToLower(c));
                }
                else {
                    result.Append(c);
                }
            }

            result.Append("' expression");
            return result.ToString();
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
            if (input.Length >= 2
                && input[0] == c
                && input[input.Length - 1] == c) {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }

        #endregion
    }
}