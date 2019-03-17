using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree;
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

        #region StringBuilder extensions

        public static string Join(this StringBuilder sb, string separator) {
            if (sb == null) {
                return string.Empty;
            }

            var lst = new List<string>();
            for (var i = 0; i < sb.Length; i++) {
                lst.Add(sb[i].ToString());
            }

            return string.Join(separator, lst.ToArray());
        }

        #endregion
    }

    /// <summary>
    ///     This wraps the .NET <c>StringBuilder</c> in a slightly more easy-to-use format.
    /// </summary>
    public class CodeBuilder {
        protected readonly StringBuilder Builder;

        public int Length {
            get => Builder.Length;
            set => Builder.Length = value;
        }

        public CodeBuilder() {
            Builder = new StringBuilder();
        }

        public CodeBuilder(int capacity) {
            Builder = new StringBuilder(capacity);
        }

        public CodeBuilder Append(string s) {
            Builder.Append(s);

            return this;
        }

        public CodeBuilder Append(char c) {
            Builder.Append(c);

            return this;
        }

        public CodeBuilder Append(object o) {
            Builder.Append(o);

            return this;
        }

        public static CodeBuilder operator +(CodeBuilder sb, string s) {
            return sb.Append(s);
        }

        public static CodeBuilder operator +(CodeBuilder sb, char c) {
            return sb.Append(c);
        }

        public static CodeBuilder operator +(CodeBuilder sb, object o) {
            return sb.Append(o);
        }

        public static implicit operator string(CodeBuilder sb) {
            return sb.ToString();
        }

        public string ToString(int startIndex, int length) {
            return Builder.ToString(startIndex, length);
        }

        public override string ToString() {
            return Builder.ToString();
        }
    }

    public class AxionCodeBuilder : CodeBuilder {
        public static AxionCodeBuilder operator +(AxionCodeBuilder b, SpannedRegion node) {
            return node.ToAxionCode(b);
        }

        public static AxionCodeBuilder operator +(AxionCodeBuilder b, string s) {
            b.Append(s);
            return b;
        }

        public void AppendJoin<T>(string separator, IList<T> items)
            where T : SyntaxTreeNode {
            if (items.Count > 0) {
                for (var i = 0; i < items.Count - 1; i++) {
                    items[i].ToAxionCode(this).Append(separator);
                }

                items[items.Count - 1].ToAxionCode(this);
            }
        }
    }

    public class CSharpCodeBuilder : CodeBuilder {
        public static CSharpCodeBuilder operator +(CSharpCodeBuilder b, SpannedRegion node) {
            return node.ToCSharpCode(b);
        }

        public static CSharpCodeBuilder operator +(CSharpCodeBuilder b, string s) {
            b.Append(s);
            return b;
        }

        public void AppendJoin<T>(string separator, IList<T> items)
            where T : SyntaxTreeNode {
            if (items.Count > 0) {
                for (var i = 0; i < items.Count - 1; i++) {
                    items[i].ToCSharpCode(this).Append(separator);
                }

                items[items.Count - 1].ToCSharpCode(this);
            }
        }
    }
}