using System.Collections.Generic;
using System.Linq;
using Axion.Core.Tokens;

namespace Axion.Core {
    /// <summary>
    ///     Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
    /// </summary>
    public static class Spec {
        /// <summary>
        ///     End of file mark.
        /// </summary>
        public const char EndOfStream = '\0';

        /// <summary>
        ///     End of line mark.
        /// </summary>
        public const char EndOfLine = '\n';

        public static readonly string[] Newlines = { "\r\n", "\n" };

        public static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType> {
            // testing
            { "assert", TokenType.KeywordAssert },
            // bool operators
            { "and", TokenType.KeywordAnd },
            { "or", TokenType.KeywordOr },
            { "in", TokenType.KeywordIn },
            { "is", TokenType.KeywordIs },
            { "as", TokenType.KeywordAs },
            // asynchronous
            { "async", TokenType.KeywordAsync },
            { "await", TokenType.KeywordAwait },
            // branching
            { "if", TokenType.KeywordIf },
            { "elif", TokenType.KeywordElif },
            { "else", TokenType.KeywordElse },
            { "match", TokenType.KeywordMatch },
            { "case", TokenType.KeywordCase },
            { "default", TokenType.KeywordDefault },
            // loops
            { "for", TokenType.KeywordFor },
            { "do", TokenType.KeywordDo },
            { "while", TokenType.KeywordWhile },
            { "break", TokenType.KeywordBreak },
            { "continue", TokenType.KeywordContinue },
            // exceptions
            { "try", TokenType.KeywordTry },
            { "throw", TokenType.KeywordThrow },
            { "catch", TokenType.KeywordCatch },
            { "anyway", TokenType.KeywordAnyway },
            // access modifiers
            { "public", TokenType.KeywordPublic },
            { "inner", TokenType.KeywordInner },
            { "private", TokenType.KeywordPrivate },
            // property modifiers
            { "readonly", TokenType.KeywordReadonly },
            { "react", TokenType.KeywordReact },
            { "static", TokenType.KeywordStatic },
            { "const", TokenType.KeywordConst },
            // modules
            { "use", TokenType.KeywordUse },
            { "module", TokenType.KeywordModule },
            // structures
            { "class", TokenType.KeywordClass },
            { "extends", TokenType.KeywordExtends },
            { "struct", TokenType.KeywordStruct },
            { "enum", TokenType.KeywordEnum },
            // variables
            { "var", TokenType.KeywordVar },
            { "new", TokenType.KeywordNew },
            // returns
            { "yield", TokenType.KeywordYield },
            { "return", TokenType.KeywordReturn },
            // values
            { "null", TokenType.KeywordNull },
            { "self", TokenType.KeywordSelf },
            { "true", TokenType.KeywordTrue },
            { "false", TokenType.KeywordFalse }
        };

        internal static readonly char[] StringQuotes = { '"', '\'' };

        internal static readonly char[] RestrictedIdentifierEndings = { '-' };

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

        internal static readonly Dictionary<char, StringLiteralOptions> StringPrefixes = new Dictionary<char, StringLiteralOptions> {
            { 'f', StringLiteralOptions.Format },
            { 'r', StringLiteralOptions.Raw }
        };

        internal static readonly char[] NumberPostfixes = {
            'l', 'L',
            'i', 'I',
            'u', 'U',
            'f', 'F'
        };

        internal static readonly int[] NumberIntBitRates = {
            8, 16, 32, 64, 128, 256
        };

        internal static readonly int[] NumberFloatBitRates = {
            32, 64, 128
        };

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

        internal const char   CharLiteralQuote             = '`';
        internal const string CommentOneLineStart          = "#";
        internal const string CommentMultilineStart        = "/*";
        internal const string CommentMultilineStartPattern = @"/\*";
        internal const string CommentMultilineEnd          = "*/";
        internal const string CommentMultilineEndPattern   = @"\*/";

        internal const int AssignPrecedence = 5;

        internal static readonly OperatorProperties InvalidOperatorProperties = new OperatorProperties(
            TokenType.Invalid,
            InputSide.None,
            Associativity.None,
            false, -1
        );

        internal static bool IsValidNumberPart(char c) {
            return c.IsValidHexadecimalDigit() || c == '_' || NumberPostfixes.Contains(c);
        }

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

        internal static bool IsValidIdStart(char start) {
            return char.IsLetter(start) || start == '_';
        }

        internal static bool IsValidIdChar(char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '-';
        }
    }
}