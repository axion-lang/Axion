using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Axion.Core.Tokens;

namespace Axion.Core {
    /// <summary>
    ///     Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
    /// </summary>
    public static class Spec {
        /// <summary>
        ///     End of source stream mark.
        /// </summary>
        public const char EndOfStream = '\0';

        internal const char   CharLiteralQuote         = '`';
        internal const string SingleCommentStart       = "#";
        internal const string MultiCommentStart        = "/*";
        internal const string MultiCommentStartPattern = @"/\*";
        internal const string MultiCommentEnd          = "*/";
        internal const string MultiCommentEndPattern   = @"\*/";

        internal static readonly char[] RestrictedIdentifierEndings = { '-' };

        /// <summary>
        ///     Contains all valid newline sequences.
        /// </summary>
        public static readonly string[] EndOfLines = { "\r\n", "\n" };

        /// <summary>
        ///     Contains all keywords in language.
        /// </summary>
        public static readonly Dictionary<string, KeywordType> Keywords = new Dictionary<string, KeywordType> {
            // testing
            { "assert", KeywordType.Assert },
            // bool operators
            { "and", KeywordType.And },
            { "or", KeywordType.Or },
            { "in", KeywordType.In },
            { "is", KeywordType.Is },
            { "as", KeywordType.As },
            // asynchronous
            { "async", KeywordType.Async },
            { "await", KeywordType.Await },
            // branching
            { "if", KeywordType.If },
            { "elif", KeywordType.Elif },
            { "else", KeywordType.Else },
            { "match", KeywordType.Match },
            { "case", KeywordType.Case },
            { "default", KeywordType.Default },
            // loops
            { "for", KeywordType.For },
            { "do", KeywordType.Do },
            { "while", KeywordType.While },
            { "break", KeywordType.Break },
            { "continue", KeywordType.Continue },
            // exceptions
            { "try", KeywordType.Try },
            { "raise", KeywordType.Raise },
            { "catch", KeywordType.Catch },
            { "anyway", KeywordType.Anyway },
            // access modifiers
            { "public", KeywordType.Public },
            { "inner", KeywordType.Inner },
            { "private", KeywordType.Private },
            // property modifiers
            { "readonly", KeywordType.Readonly },
            { "react", KeywordType.React },
            { "singleton", KeywordType.Singleton },
            { "static", KeywordType.Static },
            { "const", KeywordType.Const },
            // modules
            { "use", KeywordType.Use },
            { "module", KeywordType.Module },
            // structures
            { "class", KeywordType.Class },
            { "extends", KeywordType.Extends },
            { "struct", KeywordType.Struct },
            { "enum", KeywordType.Enum },
            // variables
            { "var", KeywordType.Var },
            { "new", KeywordType.New },
            // returns
            { "yield", KeywordType.Yield },
            { "return", KeywordType.Return },
            // values
            { "null", KeywordType.Null },
            { "self", KeywordType.Self },
            { "true", KeywordType.True },
            { "false", KeywordType.False }
        };

        #region Language string and character literals

        /// <summary>
        ///     Contains all valid quotes for string literals.
        /// </summary>
        internal static readonly char[] StringQuotes = { '"', '\'' };

        /// <summary>
        ///     Contains all valid escape sequences in string and character literals.
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
            { '\"', "\"" }
        };

        #endregion

        #region Language number literals

        internal static readonly string[] NumberTypes = {
            nameof(Int64),
            nameof(Double),
            nameof(BigInteger),
            nameof(Complex)
        };

        internal static readonly char[] NumberPostfixes = {
            'f', 'F', // float
            'l', 'L', // long
            'i', 'I', // int (followed by bit rate)
            'u', 'U', // unsigned
            'j', 'J'  // complex
        };

        internal const int MinNumberBitRate = 8;
        internal const int MaxNumberBitRate = 64;

        internal static readonly int[] IntegerBitRates = {
            MinNumberBitRate, 16, 32, MaxNumberBitRate
        };

        internal static readonly int[] FloatBitRates = {
            32, 64, 128
        };

        #endregion

        #region Language operators

        public static readonly Dictionary<string, OperatorProperties> Operators = new Dictionary<string, OperatorProperties> {
            //{ "**",  new OperatorProperties("**",  InputSide.Both,    false,     38) },

            //{ "->",  new OperatorProperties("->",  InputSide.Both,    false,     0) },
            //{ "<-",  new OperatorProperties("<-",  InputSide.Both,    false,     0) },

            { "(", new OperatorProperties(TokenType.OpLeftParenthesis,  InputSide.Both, Associativity.LeftToRight, false, 60) },
            { ")", new OperatorProperties(TokenType.OpRightParenthesis, InputSide.Both, Associativity.LeftToRight, false, 60) },
            { "[", new OperatorProperties(TokenType.OpLeftBracket,      InputSide.Both, Associativity.LeftToRight, false, 59) },
            { "]", new OperatorProperties(TokenType.OpRightBracket,     InputSide.Both, Associativity.LeftToRight, false, 59) },
            { ".", new OperatorProperties(TokenType.OpDot,              InputSide.Both, Associativity.LeftToRight, false, 55) },

            { "++", new OperatorProperties(TokenType.OpIncrement, InputSide.SomeOne, Associativity.RightToLeft, false, 50) },
            { "--", new OperatorProperties(TokenType.OpDecrement, InputSide.SomeOne, Associativity.RightToLeft, false, 50) },
            { "!", new OperatorProperties(TokenType.OpNot,        InputSide.Right,   Associativity.RightToLeft, false, 50) },
            { "~", new OperatorProperties(TokenType.OpBitwiseNot, InputSide.Right,   Associativity.RightToLeft, false, 50) },

            { "*", new OperatorProperties(TokenType.OpMultiply,   InputSide.Both, Associativity.LeftToRight, false, 45) },
            { "/", new OperatorProperties(TokenType.OpTrueDivide, InputSide.Both, Associativity.LeftToRight, false, 45) },
            { "%", new OperatorProperties(TokenType.OpRemainder,  InputSide.Both, Associativity.LeftToRight, false, 45) },

            { "+", new OperatorProperties(TokenType.OpAdd,      InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "-", new OperatorProperties(TokenType.OpSubtract, InputSide.Both, Associativity.LeftToRight, false, 40) },

            { "<<", new OperatorProperties(TokenType.OpLeftShift,  InputSide.Both, Associativity.LeftToRight, false, 35) },
            { ">>", new OperatorProperties(TokenType.OpRightShift, InputSide.Both, Associativity.LeftToRight, false, 35) },

            { "in", new OperatorProperties(TokenType.OpIn,                 InputSide.Both, Associativity.LeftToRight, false, 30) },
            { "<", new OperatorProperties(TokenType.OpLessThan,            InputSide.Both, Associativity.LeftToRight, false, 30) },
            { "<=", new OperatorProperties(TokenType.OpLessThanOrEqual,    InputSide.Both, Associativity.LeftToRight, false, 30) },
            { ">", new OperatorProperties(TokenType.OpGreaterThan,         InputSide.Both, Associativity.LeftToRight, false, 30) },
            { ">=", new OperatorProperties(TokenType.OpGreaterThanOrEqual, InputSide.Both, Associativity.LeftToRight, false, 30) },

            { "==", new OperatorProperties(TokenType.OpEquals,    InputSide.Both, Associativity.LeftToRight, false, 25) },
            { "!=", new OperatorProperties(TokenType.OpNotEquals, InputSide.Both, Associativity.LeftToRight, false, 25) },

            { "&", new OperatorProperties(TokenType.OpBitwiseAnd,  InputSide.Both, Associativity.LeftToRight, false, 17) },
            { "^", new OperatorProperties(TokenType.OpExclusiveOr, InputSide.Both, Associativity.LeftToRight, false, 16) },
            { "|", new OperatorProperties(TokenType.OpBitwiseOr,   InputSide.Both, Associativity.LeftToRight, false, 15) },

            { "&&", new OperatorProperties(TokenType.OpAnd, InputSide.Both, Associativity.LeftToRight, false, 11) },
            { "||", new OperatorProperties(TokenType.OpOr,  InputSide.Both, Associativity.LeftToRight, false, 10) },

            { "=", new OperatorProperties(TokenType.OpAssign,            InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "+=", new OperatorProperties(TokenType.OpAddEqual,         InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "-=", new OperatorProperties(TokenType.OpSubtractEqual,    InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "*=", new OperatorProperties(TokenType.OpMultiplyEqual,    InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "/=", new OperatorProperties(TokenType.OpTrueDivideEqual,  InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "%=", new OperatorProperties(TokenType.OpRemainderEqual,   InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "<<=", new OperatorProperties(TokenType.OpLeftShiftEqual,  InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { ">>=", new OperatorProperties(TokenType.OpRightShiftEqual, InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "&=", new OperatorProperties(TokenType.OpBitwiseAndEqual,  InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "|=", new OperatorProperties(TokenType.OpBitwiseOrEqual,   InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "^=", new OperatorProperties(TokenType.OpExclusiveOrEqual, InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },
            { "=>", new OperatorProperties(TokenType.OpRightArrow,       InputSide.Both, Associativity.RightToLeft, false, AssignPrecedence) },

            { ",", new OperatorProperties(TokenType.OpComma,     InputSide.Both, Associativity.LeftToRight, false, 3) },
            { ";", new OperatorProperties(TokenType.OpSemicolon, InputSide.Both, Associativity.LeftToRight, false, 2) },
            { ":", new OperatorProperties(TokenType.OpColon,     InputSide.Both, Associativity.LeftToRight, false, 1) },

            { "{", new OperatorProperties(TokenType.OpLeftBrace,  InputSide.Both, Associativity.LeftToRight, false, 0) },
            { "}", new OperatorProperties(TokenType.OpRightBrace, InputSide.Both, Associativity.LeftToRight, false, 0) }
        };

        public static readonly string[] OperatorsValues = Operators.Keys.OrderByDescending(val => val.Length).ToArray();

        public static readonly char[] OperatorChars = OperatorsValues.Select(val => val[0]).ToArray();

        internal const int AssignPrecedence = 5;

        internal static readonly OperatorProperties InvalidOperatorProperties = new OperatorProperties(
            TokenType.Invalid,
            InputSide.None,
            Associativity.None,
            false, -1
        );

        #endregion

        #region Extensions for character checking

        internal static bool IsValidOctalDigit(this char c) {
            return c == '0' || c == '1' || c == '2' || c == '3'
                || c == '4' || c == '5' || c == '6' || c == '7';
        }

        internal static bool IsValidHexadecimalDigit(this char c) {
            return c == '0' || c == '1' || c == '2' || c == '3' || c == '4'
                || c == '5' || c == '6' || c == '7' || c == '8' || c == '9'
                || c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f'
                || c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F';
        }

        internal static bool IsSpaceOrTab(char c) {
            return c == ' ' || c == '\t';
        }

        internal static bool IsLetterOrNumberPart(char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        internal static bool IsValidNumberPart(char c) {
            return c.IsValidHexadecimalDigit() || c == '_' || c == '.' || NumberPostfixes.Contains(c);
        }

        internal static bool IsValidIdStart(char start) {
            return char.IsLetter(start) || start == '_';
        }

        internal static bool IsValidIdChar(char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '-';
        }

        #endregion
    }
}