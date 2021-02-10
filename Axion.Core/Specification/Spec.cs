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

        public static bool IsValidIdStart(this char c) {
            return char.IsLetter(c) || c == '_';
        }

        public static bool IsValidIdNonEnd(this char c) {
            return c == '-';
        }

        public static bool IsValidIdEnd(this char c) {
            return c.IsValidIdStart() || char.IsDigit(c);
        }

        public static bool IsValidIdPart(this char c) {
            return c.IsValidIdEnd() || c.IsValidIdNonEnd();
        }

        // @formatter:off

        public static readonly Dictionary<char, string> EscapeSequences = new() {
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

        public static readonly Dictionary<string,(TokenType Type, int Precedence, InputSide Side)>
            Operators = new() {
            { "of",      (Of,                      19,  Unknown) },
            { ".",       (Dot,                     19,  Unknown) },

            { "++",      (DoublePlus,              18,  Unknown) },
            { "--",      (DoubleMinus,             18,  Unknown) },

            { "**",      (DoubleStar,              17,  Both) },

            { "not",     (Not,                     16,  Right) },
            { "~",       (Tilde,                   16,  Right) },

            { "*",       (Star,                    15,  Both) },
            { "/",       (Slash,                   15,  Both) },
            { "//",      (DoubleSlash,             15,  Both) },
            { "%",       (Percent,                 15,  Both) },

            { "+",       (Plus,                    14,  Unknown) },
            { "-",       (Minus,                   14,  Unknown) },

            { "<<",      (DoubleLeftAngle,         13,  Both) },
            { ">>",      (DoubleRightAngle,        13,  Both) },

            { "...",     (TripleDot,               12,  Both) },
            { "..",      (DoubleDot,               12,  Both) },

            { "<=>",     (LeftRightFatArrow,       11,  Both) },

            { "<",       (LeftAngle,               10,  Both) },
            { "<=",      (LeftAngleEquals,         10,  Both) },
            { ">",       (RightAngle,              10,  Both) },
            { ">=",      (RightAngleEquals,        10,  Both) },

            { "==",      (DoubleEquals,            9,   Both) },
            { "!=",      (ExclamationEquals,       9,   Both) },

            { "&",       (Ampersand,               8,   Both) },
            { "^",       (Caret,                   7,   Both) },
            { "|",       (Pipe,                    6,   Both) },

            { "is",      (Is,                      5,   Both) },
            { "in",      (In,                      5,   Both) },

            { "|>",      (PipeRightAngle,          4,   Both) },

            { "and",     (And,                     3,   Both) },

            { "or",      (Or,                      2,   Both) },

            { "??",      (DoubleQuestion,          1,   Both) },

            { "=",       (EqualsSign,              0,   Both) },
            { "+=",      (PlusEquals,              0,   Both) },
            { "-=",      (MinusEquals,             0,   Both) },
            { "**=",     (DoubleStarEquals,        0,   Both) },
            { "*=",      (StarEquals,              0,   Both) },
            { "/=",      (SlashEquals,             0,   Both) },
            { "//=",     (DoubleSlashEquals,       0,   Both) },
            { "%=",      (PercentEquals,           0,   Both) },
            { "?=",      (QuestionEquals,          0,   Both) },
            { "<<=",     (DoubleLeftAngleEquals,   0,   Both) },
            { ">>=",     (DoubleRightAngleEquals,  0,   Both) },
            { "&=",      (AmpersandEquals,         0,   Both) },
            { "|=",      (PipeEquals,              0,   Both) },
            { "^=",      (CaretEquals,             0,   Both) },
            
            { "<->",     (LeftRightArrow,          -1,  Both) }
        };

        public static readonly string[] OperatorsKeys =
            Operators.Keys.OrderByDescending(k => k.Length).ToArray();

        public static readonly Regex NonIndentRegex = new(
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
            DoublePlus,
            DoubleMinus,
            Plus,
            Minus,
            Not,
            Tilde
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

        public static readonly Dictionary<string, TokenType> Punctuation = new() {
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
        
        // @formatter:on

        public static readonly string[] PunctuationKeys = Punctuation.Keys.ToArray();

        /// <summary>
        ///     Token types that can't start an expression.
        /// </summary>
        public static readonly TokenType[] NeverExprStartTypes = Operators
            .Values.Where(op => op.Side == Both)
            .Select(op => op.Type)
            .Union(
                CloseBrace,
                CloseBracket,
                CloseParenthesis,
                Comma,
                DoubleCloseBrace,
                KeywordElif,
                KeywordElse,
                LeftArrow,
                Outdent,
                Question,
                RightArrow
            );

        public const string CharType = "Char";
        public const string StringType = "String";
        public const string VoidType = "void";
        public const string UnitType = "Unit";
        public const string UnionType = "Union";
        public const string UnknownType = "UNKNOWN_TYPE";

        public static readonly Dictionary<string, Func<Node, Expr>>
            ParsingFunctions = new() {
                { "Expr", AnyExpr.Parse },
                { nameof(AnyExpr), AnyExpr.Parse },
                { nameof(InfixExpr), InfixExpr.Parse },
                { nameof(PrefixExpr), PrefixExpr.Parse },
                { nameof(PostfixExpr), PostfixExpr.Parse },
                { nameof(AtomExpr), AtomExpr.Parse },
                { nameof(ConstantExpr), ConstantExpr.ParseNew }
            };

        public static readonly Dictionary<string, Type> ParsingTypes = new() {
            { nameof(ScopeExpr), typeof(ScopeExpr) },
            { nameof(TypeName), typeof(TypeName) }
        };
    }
}
