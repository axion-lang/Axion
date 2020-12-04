using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.InputSide;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    /// <summary>
    ///     Static class with language's grammar definitions
    ///     (allowed operators, keywords, etc.)
    /// </summary>
    public static partial class Spec {
        public const string FileExtension = ".ax";
        
        public const char EndOfCode = '\0';
        public const string OneLineCommentMark = "#";
        public const string MultiLineCommentMark = "###";
        public const string CharacterQuote = "`";

        public static readonly char[] Eols = {
            '\r',
            '\n'
        };

        public static readonly char[] StringQuotes = {
            '"',
            '\''
        };

        public static readonly char[] StringPrefixes = {
            'r',
            'f'
        };

        public static readonly char[] White = {
            ' ',
            '\t'
        };

        // @formatter:off

        public static readonly char[] NumbersDec = {
            '0', '1', '2', '3', '4',
            '5', '6', '7', '8', '9'
        };

        public static readonly char[] NumbersHex = NumbersDec.Union(
            'a', 'b', 'c', 'd', 'e', 'f',
            'A', 'B', 'C', 'D', 'E', 'F'
        );
        
        // @formatter:on

        public static bool IsIdStart(this char c) {
            return char.IsLetter(c) || c == '_';
        }

        public static bool IsIdNonEnd(this char c) {
            return c == '-';
        }

        public static bool IsIdAfterNonEnd(this char c) {
            return c.IsIdStart() || char.IsDigit(c);
        }

        public static bool IsIdPart(this char c) {
            return c.IsIdAfterNonEnd() || c.IsIdNonEnd();
        }

        // @formatter:off

        public static readonly Dictionary<char, string> EscapeSequences =
        new Dictionary<char, string> {
            { '0', "\u0000" },
            { 'a', "\u0007" },
            { 'b', "\u0008" },
            { 'f', "\u000c" },
            { 'n', "\u000a" },
            { 'r', "\u000d" },
            { 't', "\u0009" },
            { 'v', "\u000b" },
            { '\\', "\\" },
            { '\"', "\"" },
            { '\'', "\'" }
        };

        public static readonly Dictionary<
            string,
            (TokenType Type, int Precedence, InputSide Side)
        > Operators = new Dictionary<string, (TokenType, int, InputSide)> {
            { "of",      (OpOf,                 19,  Unknown) },
            { ".",       (OpDot,                19,  Unknown) },

            { "++",      (OpIncrement,          18,  Unknown) },
            { "--",      (OpDecrement,          18,  Unknown) },

            { "**",      (OpPower,              17,  Both) },

            { "not",     (OpNot,                16,  Right) },
            { "~",       (OpBitNot,             16,  Right) },

            { "*",       (OpMultiply,           15,  Both) },
            { "/",       (OpTrueDivide,         15,  Both) },
            { "//",      (OpFloorDivide,        15,  Both) },
            { "%",       (OpRemainder,          15,  Both) },

            { "+",       (OpPlus,               14,  Unknown) },
            { "-",       (OpMinus,              14,  Unknown) },

            { "<<",      (OpBitLeftShift,       13,  Both) },
            { ">>",      (OpBitRightShift,      13,  Both) },

            { "...",     (Op3Dot,               12,  Both) },
            { "..",      (Op2Dot,               12,  Both) },

            { "<=>",     (Op3WayCompare,        11,  Both) },

            { "<",       (OpLess,               10,  Both) },
            { "<=",      (OpLessOrEqual,        10,  Both) },
            { ">",       (OpGreater,            10,  Both) },
            { ">=",      (OpGreaterOrEqual,     10,  Both) },

            { "==",      (OpEqualsEquals,       9,   Both) },
            { "!=",      (OpNotEquals,          9,   Both) },

            { "&",       (OpBitAnd,             8,   Both) },
            { "^",       (OpBitXor,             7,   Both) },
            { "|",       (OpBitOr,              6,   Both) },

            { "is",      (OpIs,                 5,   Both) },
            { "in",      (OpIn,                 5,   Both) },

            { "|>",      (RightPipeline,        4,   Both) },

            { "and",     (OpAnd,                3,   Both) },

            { "or",      (OpOr,                 2,   Both) },

            { "??",      (Op2Question,          1,   Both) },

            { "=",       (OpAssign,             0,   Both) },
            { "+=",      (OpPlusAssign,         0,   Both) },
            { "-=",      (OpMinusAssign,        0,   Both) },
            { "**=",     (OpPowerAssign,        0,   Both) },
            { "*=",      (OpMultiplyAssign,     0,   Both) },
            { "/=",      (OpFloorDivideAssign,  0,   Both) },
            { "//=",     (OpTrueDivideAssign,   0,   Both) },
            { "%=",      (OpRemainderAssign,    0,   Both) },
            { "?=",      (OpQuestionAssign,     0,   Both) },
            { "<<=",     (OpBitLShiftAssign,    0,   Both) },
            { ">>=",     (OpBitRShiftAssign,    0,   Both) },
            { "&=",      (OpBitAndAssign,       0,   Both) },
            { "|=",      (OpBitOrAssign,        0,   Both) },
            { "^=",      (OpBitXorAssign,       0,   Both) }
        };

        public static readonly string[] OperatorsKeys =
            Operators.Keys.OrderByDescending(k => k.Length).ToArray();

        public static readonly Regex NonIndentRegex = new Regex(
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
                })
                .ToDictionary(
                    x => x.Item1,
                    x => x.Item2
                );
        
        // @formatter:on

        /// <summary>
        ///     Token types that are applicable on the left side of expression.
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
        ///     Token types that can start a scope expression.
        /// </summary>
        public static readonly TokenType[] ScopeStartMarks = {
            OpenBrace,
            Indent
        };

        public static readonly TokenType[] Constants = {
            TokenType.String,
            Character,
            Number,
            KeywordTrue,
            KeywordFalse,
            KeywordNil
        };

        // @formatter:off

        public static readonly Dictionary<string, TokenType> Punctuation =
            new Dictionary<string, TokenType> {
                // Order of keys makes sense here.
                // Longer tokens must be above shorter ones.
                // (Lexer's character stream functions
                //  work correctly only with Dict-s sorted by length.)
                { "->", RightArrow },
                { "<-", LeftArrow },
                { "{{", DoubleOpenBrace },
                { "}}", DoubleCloseBrace },
                { "@",  At },
                { "?",  Question },
                { "$",  Dollar },
                { "(",  OpenParenthesis },
                { ")",  CloseParenthesis },
                { "[",  OpenBracket },
                { "]",  CloseBracket },
                { "{",  OpenBrace },
                { "}",  CloseBrace },
                { ",",  Comma },
                { ":",  Colon },
                { ";",  Semicolon }
            };

        public static readonly string[] PunctuationKeys = Punctuation.Keys.ToArray();

        /// <summary>
        ///     Token types that can't start an expression.
        /// </summary>
        public static readonly TokenType[] NeverExprStartTypes =
            Operators.Values
                     .Where(op => op.Side == Both)
                     .Select(op => op.Type)
                     .Union(
                         Outdent, Comma, Question,
                         CloseBrace, DoubleCloseBrace, CloseBracket, CloseParenthesis,
                         KeywordElif, KeywordElse,
                         LeftArrow, RightArrow
                     );

        public const string CharType   = "Char";
        public const string StringType = "String";
        public const string VoidType = "void";
        public const string UnitType = "Unit";
        public const string UnionType = "Union";
        public const string UnknownType = "UNKNOWN_TYPE";

        public static readonly Dictionary<string, Func<Node, Expr>> ParsingFunctions =
            new Dictionary<string, Func<Node, Expr>> {
                { "Expr",                AnyExpr.Parse },
                { nameof(AnyExpr),       AnyExpr.Parse },
                { nameof(InfixExpr),     InfixExpr.Parse },
                { nameof(PrefixExpr),    PrefixExpr.Parse },
                { nameof(PostfixExpr),   PostfixExpr.Parse },
                { nameof(AtomExpr),      AtomExpr.Parse },
                { nameof(ConstantExpr),  ConstantExpr.ParseNew }
            };
        
        public static readonly Dictionary<string, Type> ParsingTypes =
            new Dictionary<string, Type> {
                { nameof(ScopeExpr), typeof(ScopeExpr) },
                { nameof(TypeName),  typeof(TypeName) }
            };
        
        // @formatter:on
    }
}
