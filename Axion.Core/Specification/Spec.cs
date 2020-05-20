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

        public static readonly Dictionary<string, (TokenType Type, int Precedence, InputSide Side)> Operators =
        new Dictionary<string, (TokenType, int, InputSide)> {
            { "of",      (OpOf,                 19,  InputSide.Unknown) }, 
            { ".",       (OpDot,                19,  InputSide.Unknown) }, 

            { "++",      (OpIncrement,          18,  InputSide.Unknown) }, 
            { "--",      (OpDecrement,          18,  InputSide.Unknown) }, 

            { "**",      (OpPower,              17,  InputSide.Both) },    

            { "not",     (OpNot,                16,  InputSide.Right) },   
            { "~",       (OpBitNot,             16,  InputSide.Right) },   

            { "*",       (OpMultiply,           15,  InputSide.Both) },    
            { "/",       (OpTrueDivide,         15,  InputSide.Both) },    
            { "//",      (OpFloorDivide,        15,  InputSide.Both) },    
            { "%",       (OpRemainder,          15,  InputSide.Both) },    

            { "+",       (OpPlus,               14,  InputSide.Unknown) }, 
            { "-",       (OpMinus,              14,  InputSide.Unknown) }, 

            { "<<",      (OpBitLeftShift,       13,  InputSide.Both) },    
            { ">>",      (OpBitRightShift,      13,  InputSide.Both) },    

            { "...",     (Op3Dot,               12,  InputSide.Both) },    
            { "..",      (Op2Dot,               12,  InputSide.Both) },    

            { "<=>",     (Op3WayCompare,        11,  InputSide.Both) },    

            { "<",       (OpLess,               10,  InputSide.Both) },    
            { "<=",      (OpLessOrEqual,        10,  InputSide.Both) },    
            { ">",       (OpGreater,            10,  InputSide.Both) },    
            { ">=",      (OpGreaterOrEqual,     10,  InputSide.Both) },    

            { "==",      (OpEqualsEquals,       9,   InputSide.Both) },    
            { "!=",      (OpNotEquals,          9,   InputSide.Both) },    

            { "&",       (OpBitAnd,             8,   InputSide.Both) },    
            { "^",       (OpBitXor,             7,   InputSide.Both) },    
            { "|",       (OpBitOr,              6,   InputSide.Both) },    

            { "is",      (OpIs,                 5,   InputSide.Both) },    
            { "is-not",  (OpIsNot,              5,   InputSide.Both) },    
            { "in",      (OpIn,                 5,   InputSide.Both) },    
            { "not-in",  (OpNotIn,              5,   InputSide.Both) },    

            { "|>",      (RightPipeline,        4,   InputSide.Both) },    

            { "and",     (OpAnd,                3,   InputSide.Both) },    

            { "or",      (OpOr,                 2,   InputSide.Both) },    

            { "??",      (Op2Question,          1,   InputSide.Both) },    

            { "=",       (OpAssign,             0,   InputSide.Both) },    
            { "+=",      (OpPlusAssign,         0,   InputSide.Both) },    
            { "-=",      (OpMinusAssign,        0,   InputSide.Both) },    
            { "**=",     (OpPowerAssign,        0,   InputSide.Both) },    
            { "*=",      (OpMultiplyAssign,     0,   InputSide.Both) },    
            { "/=",      (OpFloorDivideAssign,  0,   InputSide.Both) },    
            { "//=",     (OpTrueDivideAssign,   0,   InputSide.Both) },    
            { "%=",      (OpRemainderAssign,    0,   InputSide.Both) },    
            { "?=",      (OpQuestionAssign,     0,   InputSide.Both) },    
            { "<<=",     (OpBitLShiftAssign,    0,   InputSide.Both) },    
            { ">>=",     (OpBitRShiftAssign,    0,   InputSide.Both) },    
            { "&=",      (OpBitAndAssign,       0,   InputSide.Both) },    
            { "|=",      (OpBitOrAssign,        0,   InputSide.Both) },    
            { "^=",      (OpBitXorAssign,       0,   InputSide.Both) }
        };
        
        // @formatter:on

        public static readonly string[] OperatorsKeys =
            Operators.Keys.OrderByDescending(k => k.Length).ToArray();

        public static readonly Regex NonIndentRegex = new Regex(
            $@"^(?:{string.Join("|", OperatorsKeys.Select(Regex.Escape))})[^\w\d]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        /// <summary>
        ///     Maps language keyword's string value to it's token type.
        /// </summary>
        public static readonly Dictionary<string, TokenType> Keywords = Enum
                                                                        .GetNames(typeof(TokenType))
                                                                        .Where(
                                                                            n => n.StartsWith(
                                                                                "Keyword"
                                                                            )
                                                                        )
                                                                        .Select(
                                                                            n => {
                                                                                Enum.TryParse(
                                                                                    n,
                                                                                    out TokenType
                                                                                        type
                                                                                );
                                                                                return (
                                                                                    n.Remove(0, 7)
                                                                                     .ToLower(),
                                                                                    type);
                                                                            }
                                                                        )
                                                                        .ToDictionary(
                                                                            x => x.Item1,
                                                                            x => x.Item2
                                                                        );

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
            OpenBrace, Indent
        };

        internal static readonly TokenType[] Constants = {
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
        internal static readonly TokenType[] NeverExprStartTypes =
            Operators.Values
                     .Where(op => op.Side == InputSide.Both)
                     .Select(op => op.Type)
                     .Union(
                         Outdent, End, Comma, Question,
                         CloseBrace, DoubleCloseBrace, CloseBracket, CloseParenthesis,
                         KeywordElif, KeywordElse,
                         LeftArrow, RightArrow
                     );

        internal static readonly TypeName CharType   = new SimpleTypeName("Char");
        internal static readonly TypeName StringType = new SimpleTypeName("String");

        internal static readonly Dictionary<string, Func<Node, Expr>> ParsingFunctions =
            new Dictionary<string, Func<Node, Expr>> {
                { "Expr",          AnyExpr.Parse },
                { "AnyExpr",       AnyExpr.Parse },
                { "InfixExpr",     InfixExpr.Parse },
                { "PrefixExpr",    PrefixExpr.Parse },
                { "PostfixExpr",   PostfixExpr.Parse },
                { "AtomExpr",      AtomExpr.Parse },
                { "ConstantExpr",  ConstantExpr.ParseNew }
            };
        
        internal static readonly Dictionary<string, Type> ParsingTypes =
            new Dictionary<string, Type> {
                { "ScopeExpr", typeof(ScopeExpr) },
                { "TypeName",  typeof(TypeName) }
            };
        
        // @formatter:on
    }
}
