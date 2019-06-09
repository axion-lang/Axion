using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.TokenType;
using OP = Axion.Core.Processing.Lexical.Tokens.OperatorProperties;

namespace Axion.Core.Specification {
    public partial class Spec {
        /// <summary>
        ///     Maps language keyword's string value
        ///     to it's token type.
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
        ///     Maps language operator's string value
        ///     to it's operator properties.
        /// </summary>
        public static readonly Dictionary<string, OP> Operators
            = new Dictionary<string, OP> {
            { "++",     new OP(OpIncrement,         16, InputSide.Unknown) },
            { "--",     new OP(OpDecrement,         16, InputSide.Unknown) },

            { "**",     new OP(OpPower,             15) },

            { "not",    new OP(OpNot,               14, InputSide.Right) },
            { "~",      new OP(OpBitNot,            14, InputSide.Right) },

            { "*",      new OP(OpMultiply,          13) },
            { "/",      new OP(OpTrueDivide,        13) },
            { "//",     new OP(OpFloorDivide,       13) },
            { "%",      new OP(OpRemainder,         13) },

            { "+",      new OP(OpPlus,              12, InputSide.Unknown) },
            { "-",      new OP(OpMinus,             12, InputSide.Unknown) },

            { "<<",     new OP(OpBitLeftShift,      11) },
            { ">>",     new OP(OpBitRightShift,     11) },

            { "<=>",    new OP(Op3WayCompare,       10) },
            
            { "<",      new OP(OpLess,              9)  },
            { "<=",     new OP(OpLessOrEqual,       9)  },
            { ">",      new OP(OpGreater,           9)  },
            { ">=",     new OP(OpGreaterOrEqual,    9)  },
            
            { "==",     new OP(OpEqualsEquals,      8)  },
            { "!=",     new OP(OpNotEquals,         8)  },

            { "&",      new OP(OpBitAnd,            7)  },
            { "^",      new OP(OpBitXor,            6)  },
            { "|",      new OP(OpBitOr,             5)  },

            //{ "as",     new OP(OpAs,                4)  },
            { "is",     new OP(OpIs,                4)  },
            { "is-not", new OP(OpIsNot,             4)  },
            { "in",     new OP(OpIn,                4)  },
            { "not-in", new OP(OpNotIn,             4)  },

            // infix function call here
            
            { "and",    new OP(OpAnd,               3)  },
            { "&&",     new OP(OpAnd,               3)  },

            { "or",     new OP(OpOr,                2)  },
            { "||",     new OP(OpOr,                2)  },

            { "??",     new OP(Op2Question,         1)  },

            { "=",      new OP(OpAssign,            0)  },
            { "+=",     new OP(OpPlusAssign,        0)  },
            { "-=",     new OP(OpMinusAssign,       0)  },
            { "**=",    new OP(OpPowerAssign,       0)  },
            { "*=",     new OP(OpMultiplyAssign,    0)  },
            { "/=",     new OP(OpFloorDivideAssign, 0)  },
            { "//=",    new OP(OpTrueDivideAssign,  0)  },
            { "%=",     new OP(OpRemainderAssign,   0)  },
            { "?=",     new OP(OpQuestionAssign,    0)  },
            { "<<=",    new OP(OpBitLShiftAssign,   0)  },
            { ">>=",    new OP(OpBitRShiftAssign,   0)  },
            { "&=",     new OP(OpBitAndAssign,      0)  },
            { "|=",     new OP(OpBitOrAssign,       0)  },
            { "^=",     new OP(OpBitXorAssign,      0)  }
        };

        /// <summary>
        ///     Token types that applicable on
        ///     the left side of expression.
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
        ///     Maps language symbol's string value
        ///     to it's token type.
        /// </summary>
        internal static readonly Dictionary<string, TokenType> Symbols
            = new Dictionary<string, TokenType> {
            { ".",      Dot },
            { "|>",     RightPipeline },
            { "<|",     LeftPipeline },
            { "=>",     RightFatArrow },
            { "@",      At },
            { "?",      Question },
            { "::",     ColonColon },

            { "(",      OpenParenthesis },
            { ")",      CloseParenthesis },
            { "[",      OpenBracket },
            { "]",      CloseBracket },
            { "{",      OpenBrace },
            { "}",      CloseBrace },
            { "{{",     DoubleOpenBrace },
            { "}}",     DoubleCloseBrace },
            { ",",      Comma },
            { ":",      Colon },
            { ";",      Semicolon }
        };


        /// <summary>
        ///     Contains all operators and symbols.
        ///     Sorted by descending.
        /// </summary>
        public static readonly string[] SortedSymbolics =
            Operators
                .Keys
                .Union(Symbols.Keys)
                .OrderByDescending(val => val.Length)
                .ToArray();

        /// <summary>
        ///     Contains all characters that start
        ///     an operator or symbol.
        /// </summary>
        public static readonly char[] SymbolicChars =
             SortedSymbolics
                 .Select(val => val[0])
                 .Distinct()
                 .Where(c => !char.IsLetter(c))
                 .ToArray();
    }
}