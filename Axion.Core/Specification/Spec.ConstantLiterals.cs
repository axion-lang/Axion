using System.Collections.Generic;

namespace Axion.Core.Specification {
    public partial class Spec {
        // =============================
        // String and character literals
        // =============================
        
        /// <summary>
        ///     Max. count of digits in escape
        ///     sequence for Unicode char.
        /// </summary>
        internal const int  Unicode32BitHexLength = 6;
        
        /// <summary>
        ///     Character used for escaping sequences in string/char literals.
        /// </summary>
        internal const char EscapeMark = '\\';

        /// <summary>
        ///     Quote used to specify start/end
        ///     of char literal.
        /// </summary>
        internal const char CharacterLiteralQuote = '`';

        /// <summary>
        ///     Contains all valid quotes for string literals.
        /// </summary>
        internal static readonly char[] StringQuotes = { '"', '\'' };

        /// <summary>
        ///     Contains all valid escape sequences in string/char literals.
        /// </summary>
        internal static readonly Dictionary<char, string> EscapeSequences = new Dictionary<char, string> {
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

        // ===============
        // Number literals
        // ===============
        
        internal const int MinNumberBitRate = 8;
        internal const int MaxNumberBitRate = 64;

        internal static readonly int[] IntegerBitRates = {
            MinNumberBitRate, 16, 32, MaxNumberBitRate
        };

        internal static readonly int[] FloatBitRates = { 32, 64, 128 };

        internal static readonly char[] NumberPostfixes = {
            'f', 'F', // float
            'l', 'L', // long
            'i', 'I', // int (followed by bit rate)
            'u', 'U', // unsigned
            'j', 'J'  // complex
        };
    }
}