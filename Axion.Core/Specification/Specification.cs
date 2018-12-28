using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;

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

        internal const string SingleCommentStart       = "#";
        internal const string MultiCommentStart        = "#|";
        internal const string MultiCommentStartPattern = @"#\|";
        internal const string MultiCommentEnd          = "|#";
        internal const string MultiCommentEndPattern   = @"\|#";

        internal static readonly char[] RestrictedIdentifierEndings = { '-' };

        /// <summary>
        ///     Contains all valid newline sequences.
        /// </summary>
        public static readonly string[] EndOfLines = { "\r\n", "\n" };

        /// <summary>
        ///     Contains all language keywords.
        /// </summary>
        public static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType> {
            // testing
            { "assert", TokenType.KeywordAssert },
            // bool operators
            { "not", TokenType.KeywordNot },
            { "and", TokenType.KeywordAnd },
            { "or", TokenType.KeywordOr },
            { "in", TokenType.KeywordIn },
            { "not in", TokenType.KeywordNotIn },
            { "is", TokenType.KeywordIs },
            { "is not", TokenType.KeywordIsNot },
            { "as", TokenType.KeywordAs },
            // branching
            { "if", TokenType.KeywordIf },
            { "elif", TokenType.KeywordElseIf },
            { "else", TokenType.KeywordElse },
            { "match", TokenType.KeywordMatch },
            { "case", TokenType.KeywordCase },
            { "default", TokenType.KeywordDefault },
            // loops
            { "for", TokenType.KeywordFor },
            { "do", TokenType.KeywordDo },
            { "while", TokenType.KeywordWhile },
            { "break", TokenType.KeywordBreak },
            { "nobreak", TokenType.KeywordNoBreak },
            { "continue", TokenType.KeywordContinue },
            // exceptions
            { "try", TokenType.KeywordTry },
            { "raise", TokenType.KeywordRaise },
            { "catch", TokenType.KeywordCatch },
            { "anyway", TokenType.KeywordAnyway },
            // access modifiers
            { "public", TokenType.KeywordPublic },
            { "inner", TokenType.KeywordInner },
            { "private", TokenType.KeywordPrivate },
            // property modifiers
            { "readonly", TokenType.KeywordReadonly },
            { "react", TokenType.KeywordReact },
            { "singleton", TokenType.KeywordSingleton },
            { "static", TokenType.KeywordStatic },
            { "const", TokenType.KeywordConst },
            // asynchronous
            { "async", TokenType.KeywordAsync },
            { "await", TokenType.KeywordAwait },
            // modules
            { "use", TokenType.KeywordUse },
            { "namespace", TokenType.KeywordNamespace },
            { "mixin", TokenType.KeywordMixin },
            { "from", TokenType.KeywordFrom },
            // structures
            { "class", TokenType.KeywordClass },
            { "extends", TokenType.KeywordExtends },
            { "struct", TokenType.KeywordStruct },
            { "enum", TokenType.KeywordEnum },
            { "lambda", TokenType.KeywordLambda },
            // variables
            { "var", TokenType.KeywordVar },
            { "new", TokenType.KeywordNew },
            { "delete", TokenType.KeywordDelete },
            // returns
            { "yield", TokenType.KeywordYield },
            { "return", TokenType.KeywordReturn },
            { "pass", TokenType.KeywordPass },
            // values
            { "null", TokenType.KeywordNull },
            { "self", TokenType.KeywordSelf },
            { "true", TokenType.KeywordTrue },
            { "false", TokenType.KeywordFalse },
            { "with", TokenType.KeywordWith }
        };

        internal static readonly TokenType[] NeverTestTypes = {
            TokenType.Assign,
            TokenType.AddAssign,
            TokenType.SubtractAssign,
            TokenType.MultiplyAssign,
            TokenType.TrueDivideAssign,
            TokenType.RemainderAssign,
            TokenType.BitAndAssign,
            TokenType.BitOrAssign,
            TokenType.BitExclusiveOrAssign,
            TokenType.BitLeftShiftAssign,
            TokenType.BitRightShiftAssign,
            TokenType.PowerAssign,
            TokenType.FloorDivideAssign,

            TokenType.Indent,
            TokenType.Outdent,
            TokenType.Newline,
            TokenType.EndOfStream,
            TokenType.Semicolon,

            TokenType.RightBrace,
            TokenType.RightBracket,
            TokenType.RightParenthesis,

            TokenType.Comma,

            TokenType.KeywordFor,
            TokenType.KeywordIn,
            TokenType.KeywordIf
        };
    }
}