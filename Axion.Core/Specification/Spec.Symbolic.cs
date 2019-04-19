using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        public static readonly Dictionary<string, OperatorProperties> Operators = new Dictionary<string, OperatorProperties> {
            { "++",     new OperatorProperties(OpIncrement,            17, InputSide.Unknown) },
            { "--",     new OperatorProperties(OpDecrement,            17, InputSide.Unknown) },

            { "**",     new OperatorProperties(OpPower,                16) },

            { "not",    new OperatorProperties(OpNot,                  15, InputSide.Left) },
            { "~",      new OperatorProperties(OpBitNot,               15, InputSide.Left) },

            { "*",      new OperatorProperties(OpMultiply,             14) },
            { "/",      new OperatorProperties(OpTrueDivide,           14) },
            { "//",     new OperatorProperties(OpFloorDivide,          14) },
            { "%",      new OperatorProperties(OpRemainder,            14) },

            { "+",      new OperatorProperties(OpPlus,                 13, InputSide.Unknown) },
            { "-",      new OperatorProperties(OpMinus,                13, InputSide.Unknown) },

            { "<<",     new OperatorProperties(OpBitLeftShift,         12) },
            { ">>",     new OperatorProperties(OpBitRightShift,        12) },

            { "<=>",    new OperatorProperties(Op3WayCompare,          11) },
            { "<",      new OperatorProperties(OpLess,                 10) },
            { "<=",     new OperatorProperties(OpLessOrEqual,          10) },
            { ">",      new OperatorProperties(OpGreater,              10) },
            { ">=",     new OperatorProperties(OpGreaterOrEqual,       10) },
            { "==",     new OperatorProperties(OpEqualsEquals,         9)  },
            { "!=",     new OperatorProperties(OpNotEquals,            9)  },

            { "&",      new OperatorProperties(OpBitAnd,               8)  },
            { "^",      new OperatorProperties(OpBitXor,               7)  },
            { "|",      new OperatorProperties(OpBitOr,                6)  },

            { "as",     new OperatorProperties(OpAs,                   5)  },
            { "is",     new OperatorProperties(OpIs,                   5)  },
            { "is not", new OperatorProperties(OpIsNot,                5)  },

            { "in",     new OperatorProperties(OpIn,                   4)  },
            { "not in", new OperatorProperties(OpNotIn,                4)  },

            { "and",    new OperatorProperties(OpAnd,                  3)  },
            { "&&",     new OperatorProperties(OpAnd,                  3)  },

            { "or",     new OperatorProperties(OpOr,                   2)  },
            { "||",     new OperatorProperties(OpOr,                   2)  },

            { "??",     new OperatorProperties(Op2Question,            1)  },

            { "=",      new OperatorProperties(OpAssign,               0)  },
            { "+=",     new OperatorProperties(OpPlusAssign,           0)  },
            { "-=",     new OperatorProperties(OpMinusAssign,          0)  },
            { "**=",    new OperatorProperties(OpPowerAssign,          0)  },
            { "*=",     new OperatorProperties(OpMultiplyAssign,       0)  },
            { "/=",     new OperatorProperties(OpFloorDivideAssign,    0)  },
            { "//=",    new OperatorProperties(OpTrueDivideAssign,     0)  },
            { "%=",     new OperatorProperties(OpRemainderAssign,      0)  },
            { "?=",     new OperatorProperties(OpNullCoalescingAssign, 0)  },
            { "<<=",    new OperatorProperties(OpBitLeftShiftAssign,   0)  },
            { ">>=",    new OperatorProperties(OpBitRightShiftAssign,  0)  },
            { "&=",     new OperatorProperties(OpBitAndAssign,         0)  },
            { "|=",     new OperatorProperties(OpBitOrAssign,          0)  },
            { "^=",     new OperatorProperties(OpBitXorAssign,         0)  }
        };

        public static readonly IReadOnlyList<(TokenType type, string value)> OperatorTypes =
            Operators.Select(kvp => (kvp.Value.Type, kvp.Key)).ToList();
        
        public static readonly TokenType[] UnaryLeftOperators = {
            OpPlus,
            OpMinus,
            OpBitNot,
            OpNot,
            OpIncrement,
            OpDecrement
        };

        internal static readonly Dictionary<string, TokenType> Symbols = new Dictionary<string, TokenType> {
            { ".",  Dot },
            { "|>", RightPipeline },
            { "<|", LeftPipeline },
            { "=>", RightFatArrow },
            { "@",  At },
            { "?",  Question },
            { "::", ColonColon },

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

        /// <summary>
        ///     Symbolic = Operators + Symbols
        /// </summary>
        public static readonly string[] SortedSymbolics =
            Operators.Keys.Union(Symbols.Keys).OrderByDescending(val => val.Length).ToArray();

        public static readonly HashSet<char> SymbolicChars =
            new HashSet<char>(
            SortedSymbolics
                .Select(val => val[0])
                .Where(c => !char.IsLetterOrDigit(c))
            );
    }
}