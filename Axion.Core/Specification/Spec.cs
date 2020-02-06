using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    /// <summary>
    ///     Static class with language's grammar definitions
    ///     (allowed operators, keywords, etc.)
    /// </summary>
    public static partial class Spec {
        public const char   Eoc                  = '\0';
        public const string OneLineCommentMark   = "#";
        public const string MultiLineCommentMark = "###";
        public const string CharacterQuote       = "`";

        public static readonly char[] Eols = {
            '\r', '\n'
        };

        public static readonly char[] StringQuotes = {
            '"', '\''
        };

        public static readonly char[] StringPrefixes = {
            'r', 'f'
        };

        public static readonly char[] White = {
            ' ', '\t'
        };

        public static readonly char[] NumbersDec = {
            '0', '1', '2', '3', '4',
            '5', '6', '7', '8', '9'
        };

        public static readonly char[] NumbersHex = NumbersDec.Union(
            'a', 'b', 'c', 'd', 'e', 'f',
            'A', 'B', 'C', 'D', 'E', 'F'
        );

        public static readonly char[] IdStart = {
            'a', 'b',
            'c', 'd',
            'e', 'f',
            'g', 'h',
            'i', 'j',
            'k', 'l',
            'm', 'n',
            'o', 'p',
            'q', 'r',
            's', 't',
            'u', 'v',
            'w', 'x',
            'y', 'z',
            'A', 'B',
            'C', 'D',
            'E', 'F',
            'G', 'H',
            'I', 'J',
            'K', 'L',
            'M', 'N',
            'O', 'P',
            'Q', 'R',
            'S', 'T',
            'U', 'V',
            'W', 'X',
            'Y', 'Z',
            '_'
        };

        public static readonly char[] IdNotEnd      = { '-' };
        public static readonly char[] IdPart        = IdStart.Union(IdNotEnd).Union(NumbersDec);
        public static readonly char[] IdAfterNotEnd = IdStart.Union(NumbersDec);

        public static readonly Dictionary<string, string> EscapeSequences =
            new Dictionary<string, string> {
                { "0", "\u0000" },
                { "a", "\u0007" },
                { "b", "\u0008" },
                { "f", "\u000c" },
                { "n", "\u000a" },
                { "r", "\u000d" },
                { "t", "\u0009" },
                { "v", "\u000b" },
                { "\\", "\\" },
                { "\"", "\"" },
                { "\'", "\'" }
            };

        public static readonly Dictionary<string, TokenType> Punctuation =
            new Dictionary<string, TokenType> {
                { "->", RightArrow },
                { "<-", LeftArrow },
                { "@", At },
                { "?", Question },
                { "$", Dollar },
                { "(", OpenParenthesis },
                { ")", CloseParenthesis },
                { "[", OpenBracket },
                { "]", CloseBracket },
                { "{", OpenBrace },
                { "}", CloseBrace },
                { "{{", DoubleOpenBrace },
                { "}}", DoubleCloseBrace },
                { ",", Comma },
                { ":", Colon },
                { ";", Semicolon }
            };

        public static readonly string[] PunctuationKeys = Punctuation.Keys.ToArray();

        // @formatter:off

        public static readonly Dictionary<string, (TokenType, int, InputSide)> Operators =
        new Dictionary<string, (TokenType, int, InputSide)> {
            { "of", (OpOf,  19, InputSide.Unknown) },
            { ".",  (OpDot, 19, InputSide.Unknown) },

            { "++", (OpIncrement, 18, InputSide.Unknown) },
            { "--", (OpDecrement, 18, InputSide.Unknown) },

            { "**", (OpPower, 17, InputSide.Both) },

            { "not", (OpNot,    16, InputSide.Right) },
            { "~",   (OpBitNot, 16, InputSide.Right) },

            { "*",  (OpMultiply,    15, InputSide.Both) },
            { "/",  (OpTrueDivide,  15, InputSide.Both) },
            { "//", (OpFloorDivide, 15, InputSide.Both) },
            { "%",  (OpRemainder,   15, InputSide.Both) },

            { "+", (OpPlus,  14, InputSide.Unknown) },
            { "-", (OpMinus, 14, InputSide.Unknown) },

            { "<<", (OpBitLeftShift,  13, InputSide.Both) },
            { ">>", (OpBitRightShift, 13, InputSide.Both) },

            { "...", (Op3Dot, 12, InputSide.Both) },
            { "..",  (Op2Dot, 12, InputSide.Both) },

            { "<=>", (Op3WayCompare, 11, InputSide.Both) },

            { "<",  (OpLess,           10, InputSide.Both) },
            { "<=", (OpLessOrEqual,    10, InputSide.Both) },
            { ">",  (OpGreater,        10, InputSide.Both) },
            { ">=", (OpGreaterOrEqual, 10, InputSide.Both) },

            { "==", (OpEqualsEquals, 9, InputSide.Both) },
            { "!=", (OpNotEquals,    9, InputSide.Both) },

            { "&", (OpBitAnd, 8, InputSide.Both) },
            { "^", (OpBitXor, 7, InputSide.Both) },
            { "|", (OpBitOr,  6, InputSide.Both) },

            { "is",     (OpIs,    5, InputSide.Both) },
            { "is-not", (OpIsNot, 5, InputSide.Both) },
            { "in",     (OpIn,    5, InputSide.Both) },
            { "not-in", (OpNotIn, 5, InputSide.Both) },

            { "|>", (RightPipeline, 4, InputSide.Both) },

            { "and", (OpAnd, 3, InputSide.Both) },

            { "or", (OpOr, 2, InputSide.Both) },

            { "??", (Op2Question, 1, InputSide.Both) },

            { "=",   (OpAssign,            0, InputSide.Both) },
            { "+=",  (OpPlusAssign,        0, InputSide.Both) },
            { "-=",  (OpMinusAssign,       0, InputSide.Both) },
            { "**=", (OpPowerAssign,       0, InputSide.Both) },
            { "*=",  (OpMultiplyAssign,    0, InputSide.Both) },
            { "/=",  (OpFloorDivideAssign, 0, InputSide.Both) },
            { "//=", (OpTrueDivideAssign,  0, InputSide.Both) },
            { "%=",  (OpRemainderAssign,   0, InputSide.Both) },
            { "?=",  (OpQuestionAssign,    0, InputSide.Both) },
            { "<<=", (OpBitLShiftAssign,   0, InputSide.Both) },
            { ">>=", (OpBitRShiftAssign,   0, InputSide.Both) },
            { "&=",  (OpBitAndAssign,      0, InputSide.Both) },
            { "|=",  (OpBitOrAssign,       0, InputSide.Both) },
            { "^=",  (OpBitXorAssign,      0, InputSide.Both) }
        };
        
        // @formatter:on

        public static readonly string[] OperatorsKeys =
            Operators.Keys.OrderByDescending(k => k.Length).ToArray();

        public static readonly Regex NotIndentRegex = new Regex(
            $@"^(?:{string.Join("|", OperatorsKeys.Select(Regex.Escape))})[^\w\d]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        /// <summary>
        ///     Maps language keyword's string value to it's token type.
        /// </summary>
        public static readonly Dictionary<string, TokenType> Keywords =
            Enum.GetNames(typeof(TokenType))
                .Where(n => n.StartsWith("Keyword"))
                .Select(n => {
                        Enum.TryParse(n, out TokenType type);
                        return (n.Remove(0, 7).ToLower(), type);
                    }
                )
                .ToDictionary(x => x.Item1, x => x.Item2);

        /// <summary>
        ///     Token types that applicable on the left side of expression.
        /// </summary>
        public static readonly TokenType[] PrefixOperators = {
            OpIncrement,
            OpDecrement,
            OpPlus,
            OpMinus,
            OpNot,
            OpBitNot
        };

        /// <summary>
        ///     Token types that can start a block expression.
        /// </summary>
        public static readonly TokenType[] BlockStartMarks = {
            Colon,
            OpenBrace,
            Indent
        };

        internal static readonly TokenType[] Constants = {
            TokenType.String,
            Character,
            Number,
            KeywordTrue,
            KeywordFalse,
            KeywordNil
        };

        /// <summary>
        ///     Token types that can't start an expression.
        /// </summary>
        internal static readonly TokenType[] NeverExprStartTypes = {
            OpAssign,
            OpPlusAssign,
            OpMinusAssign,
            OpMultiplyAssign,
            OpTrueDivideAssign,
            OpRemainderAssign,
            OpBitAndAssign,
            OpBitOrAssign,
            OpBitXorAssign,
            OpBitLShiftAssign,
            OpBitRShiftAssign,
            OpPowerAssign,
            OpFloorDivideAssign,
            Outdent,
            End,
            Semicolon,
            CloseBrace,
            CloseBracket,
            CloseParenthesis,
            Comma,
            KeywordFor,
            OpIn,
            KeywordIf
        };

        internal static readonly TypeName CharType   = new SimpleTypeName("Char");
        internal static readonly TypeName StringType = new SimpleTypeName("String");
    }
}