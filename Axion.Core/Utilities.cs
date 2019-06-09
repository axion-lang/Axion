using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
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

        /// <summary>
        ///     In:  SampleExpression
        ///     Out: 'sample' expression
        /// </summary>
        internal static string GetExprFriendlyName(string expressionTypeName) {
            string exprOriginalName = expressionTypeName.Replace("Expression", "");
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

        internal static bool WriteDecorators(this CodeBuilder c, NodeList<Expression> decorators) {
            var haveAccessMod = false;
            for (var i = 0; i < decorators?.Count; i++) {
                Expression modifier = decorators[i];
                if (modifier is NameExpression n && Spec.CSharp.AccessModifiers.Contains(n.Name)) {
                    haveAccessMod = true;
                }

                c.Write(modifier, " ");
                if (i == decorators.Count - 1) {
                    c.Write(" ");
                }
                else {
                    c.Write(", ");
                }
            }

            return haveAccessMod;
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