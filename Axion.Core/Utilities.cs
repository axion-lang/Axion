using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Specification;

namespace Axion.Core;

public static class Utilities {
    const string timedFileNameFormat = "MMM-dd_HH-mm-ss";
    static readonly DateTimeFormatInfo dateTimeFormat =
        new CultureInfo("en-US").DateTimeFormat;

    /// <summary>
    ///     Creates a file name from current date and time
    ///     in format: 'yyyy-MMM-dd_HH-mm-ss'.
    /// </summary>
    internal static string ToFileName(this DateTime dt) {
        return dt.ToString(timedFileNameFormat, dateTimeFormat);
    }

    internal static T[] OfType<T>(
        this IEnumerable<T> tokens,
        TokenType           type
    ) where T : Token {
        return tokens.Where(t => t.Type == type).ToArray();
    }

    public static void ResolvePath(string path) {
        if (!Path.IsPathRooted(path)) {
            path = Path.GetFullPath(path);
        }

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    public static int? ParseInt(string value, int radix) {
        var result = 0;
        foreach (var oneChar in value.Select(HexValue)) {
            if (oneChar < radix) {
                result = result * radix + (int) oneChar;
            }
            else {
                return null;
            }
        }

        return result;
    }

    public static int? HexValue(char from) {
        if (char.IsDigit(from) && int.TryParse(from.ToString(), out var x)) {
            return x;
        }

        return from switch {
            >= 'a' and <= 'z' => from - 'a' + 10,
            >= 'A' and <= 'Z' => from - 'A' + 10,
            _                 => null
        };
    }

    #region String utilities

    /// <summary>
    ///     Splits user command line input to arguments.
    /// </summary>
    /// <returns>Collection of arguments passed into command line.</returns>
    public static IEnumerable<string> SplitLaunchArguments(string input) {
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

    static IEnumerable<string> Split(
        string           str,
        Func<char, bool> controller
    ) {
        var nextPiece = 0;
        for (var c = 0; c < str.Length; c++) {
            if (controller(str[c])) {
                yield return str.Substring(nextPiece, c - nextPiece);

                nextPiece = c + 1;
            }
        }

        yield return str[nextPiece..];
    }

    public static string TrimMatchingChars(string input, char c) {
        while (true) {
            if (input.Length >= 2 && input[0] == c && input[^1] == c) {
                input = input[1..^1];
            }
            else {
                return input;
            }
        }
    }

    public static string Multiply(this string source, int multiplier) {
        return new string(source, multiplier);
    }

    #endregion
}
