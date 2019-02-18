using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    /// <summary>
    ///     Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
    /// </summary>
    public static partial class Spec {
        public const string SourceFileExtension = ".ax";
        public const string OutputFileExtension = ".ax";

        /// <summary>
        ///     End of source stream mark.
        /// </summary>
        public const char EndOfStream = '\0';

        public const int Unicode32BitHexLength = 6;

        /// <summary>
        ///     Contains all valid newline sequences.
        /// </summary>
        public static readonly string[] EndOfLines = { "\r\n", "\n" };

        /// <summary>
        ///     Contains all language keywords.
        /// </summary>
        public static readonly Dictionary<string, TokenType> Keywords =
            new Dictionary<string, TokenType> {
                // testing
                { "assert", KeywordAssert },
                // bool operators
                { "not", KeywordNot },
                { "and", KeywordAnd },
                { "or", KeywordOr },
                { "in", KeywordIn },
                { "not in", KeywordNotIn },
                { "is", KeywordIs },
                { "is not", KeywordIsNot },
                { "as", KeywordAs },
                // branching
                { "unless", KeywordUnless },
                { "if", KeywordIf },
                { "elif", KeywordElseIf },
                { "else", KeywordElse },
                { "match", KeywordMatch },
                { "case", KeywordCase },
                { "default", KeywordDefault },
                // loops
                { "for", KeywordFor },
                { "do", KeywordDo },
                { "while", KeywordWhile },
                { "break", KeywordBreak },
                { "nobreak", KeywordNoBreak },
                { "continue", KeywordContinue },
                // exceptions
                { "try", KeywordTry },
                { "raise", KeywordRaise },
                { "catch", KeywordCatch },
                { "anyway", KeywordAnyway },
                { "const", KeywordConst },
                // asynchronous
                { "async", KeywordAsync },
                { "await", KeywordAwait },
                // modules
                { "use", KeywordUse },
                { "module", KeywordModule },
                { "mixin", KeywordMixin },
                { "from", KeywordFrom },
                // structures
                { "class", KeywordClass },
                { "extends", KeywordExtends },
                { "struct", KeywordStruct },
                { "enum", KeywordEnum },
                { "fn", KeywordFn },
                // variables
                { "var", KeywordVar },
                { "new", KeywordNew },
                { "delete", KeywordDelete },
                // returns
                { "yield", KeywordYield },
                { "return", KeywordReturn },
                { "pass", KeywordPass },
                // values
                { "nil", KeywordNil },
                { "self", KeywordSelf },
                { "true", KeywordTrue },
                { "false", KeywordFalse },
                { "with", KeywordWith }
            };

        internal const string SingleCommentStart       = "#";
        internal const string MultiCommentStart        = "#|";
        internal const string MultiCommentStartPattern = @"#\|";
        internal const string MultiCommentEnd          = "|#";
        internal const string MultiCommentEndPattern   = @"\|#";

        internal static readonly char[] RestrictedIdentifierEndings = { '-' };

        /// <summary>
        ///     Types of token, that can start a 'block'.
        /// </summary>
        internal static readonly TokenType[] BlockStarters = { Colon, Indent, LeftBrace };

        internal static readonly TokenType[] NeverTestTypes = {
            Assign,
            AddAssign,
            SubtractAssign,
            MultiplyAssign,
            TrueDivideAssign,
            RemainderAssign,
            BitAndAssign,
            BitOrAssign,
            BitExclusiveOrAssign,
            BitLeftShiftAssign,
            BitRightShiftAssign,
            PowerAssign,
            FloorDivideAssign,
            Indent,
            Outdent,
            Newline,
            EndOfCode,
            Semicolon,
            RightBrace,
            RightBracket,
            RightParenthesis,
            Comma,
            KeywordFor,
            KeywordIn,
            KeywordIf
        };
    }
}