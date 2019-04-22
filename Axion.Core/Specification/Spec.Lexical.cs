using System.Collections.Generic;

namespace Axion.Core.Specification {
    /// <summary>
    ///     Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
    /// </summary>
    public partial class Spec {
        internal const string CommentStart      = "#";
        internal const string MultiCommentStart = "#|";
        internal const string MultiCommentEnd   = "|#";
        
        /// <summary>
        ///     End of source stream mark.
        /// </summary>
        public const char EndOfCode = '\0';

        /// <summary>
        ///     Contains all valid newline sequences.
        /// </summary>
        public static readonly string[] EndOfLines = { "\r\n", "\n" };

        // =============================
        // String and character literals
        // =============================
        
        /// <summary>
        ///     Max. count of digits in escape
        ///     sequence for Unicode char.
        /// </summary>
        internal const int Unicode32BitHexLength = 6;
        
        /// <summary>
        ///     Character used for escaping sequences in string/char literals.
        /// </summary>
        internal const char EscapeMark = '\\';

        /// <summary>
        ///     Contains all valid escape sequences in string/char literals.
        /// </summary>
        internal static Dictionary<char, string> EscapeSequences { get; } = new Dictionary<char, string> {
            { '0', "\u0000" },
            { 'a', "\u0007" },
            { 'b', "\u0008" },
            { 'f', "\u000c" },
            { 'n', "\u000a" },
            { 'r', "\u000d" },
            { 't', "\u0009" },
            { 'v', "\u000b" },
            { '\\', "\\" },
            { '\'', "\'" },
            { '\"', "\"" },
            { '`', "\\`" }
        };

        internal static char[] CharQuotes { get; } = { '`' };

        internal static char[] StringQuotes { get; } = { '\'', '\"' };
        
        // ===============
        // Number literals
        // ===============
        
        internal const int MinNumberBitRate = 8;
        internal const int MaxNumberBitRate = 64;

        internal static int[] IntegerBitRates { get; } = {
            MinNumberBitRate, 16, 32, MaxNumberBitRate
        };

        internal static int[] FloatBitRates { get; } = { 32, 64, 128 };

        internal static char[] NumberPostfixes { get; } = {
            'f', 'F', // float
            'l', 'L', // long
            'i', 'I', // int (followed by bit rate)
            'u', 'U', // unsigned
            'j', 'J'  // complex
        };
        
        internal static char[] OctalDigits { get; } = {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7'
        };

        internal static char[] HexadecimalDigits { get; } = {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F'
        };

        internal static bool IsLetterOrNumberPart(char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        internal static bool IsIdStart(char start) {
            return char.IsLetter(start) || start == '_';
        }

        internal static bool IsIdPart(char c) {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        internal static bool IsNumberStart(char c) {
            return char.IsDigit(c);
        }

        internal static bool IsSpaceOrTab(this char c) {
            return c == ' ' || c == '\t';
        }
    }
}