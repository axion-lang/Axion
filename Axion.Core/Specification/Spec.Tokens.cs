using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        /// <summary>
        ///     Maps language keyword's string value to it's token type.
        /// </summary>
        public static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType> {
            // testing
            { "assert",    KeywordAssert   },
            // branching
            { "unless",    KeywordUnless   },
            { "if",        KeywordIf       },
            { "elif",      KeywordElseIf   },
            { "else",      KeywordElse     },
            { "match",     KeywordMatch    },
            { "case",      KeywordCase     },
            { "default",   KeywordDefault  },
            // loops
            { "for",       KeywordFor      },
            { "do",        KeywordDo       },
            { "while",     KeywordWhile    },
            { "break",     KeywordBreak    },
            { "nobreak",   KeywordNoBreak  },
            { "continue",  KeywordContinue },
            // exceptions
            { "try",       KeywordTry      },
            { "raise",     KeywordRaise    },
            { "catch",     KeywordCatch    },
            { "anyway",    KeywordAnyway   },
            { "const",     KeywordConst    },
            // asynchronous
            { "async",     KeywordAsync    },
            { "await",     KeywordAwait    },
            // modules
            { "use",       KeywordUse      },
            { "module",    KeywordModule   },
            { "mixin",     KeywordMixin    },
            { "from",      KeywordFrom     },
            // structures
            { "class",     KeywordClass    },
            { "extends",   KeywordExtends  },
            { "struct",    KeywordStruct   },
            { "enum",      KeywordEnum     },
            { "fn",        KeywordFn       },
            // variables
            { "let",       KeywordLet      },
            { "new",       KeywordNew      },
            { "delete",    KeywordDelete   },
            // returns
            { "yield",     KeywordYield    },
            { "return",    KeywordReturn   },
            { "pass",      KeywordPass     },
            // values
            { "nil",       KeywordNil      },
            { "true",      KeywordTrue     },
            { "false",     KeywordFalse    },
            { "with",      KeywordWith     },
            { "when",      KeywordWhen     }
        };
        
        /// <summary>
        ///     Maps language operator's string value to it's operator properties.
        /// </summary>
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

        /// <summary>
        ///     Token types that applicable to unary expression (left).
        /// </summary>
        public static readonly TokenType[] UnaryLeftOperators =
            Operators.Values
                     .Where(p => p.InputSide != InputSide.Right)
                     .Select(p => p.Type).ToArray();

        /// <summary>
        ///     Token types that applicable to assignment expression.
        /// </summary>
        public static readonly TokenType[] AssignmentOperators = 
            Operators.Values
                     .Where(
                        p => p.Type.ToString("G")
                            .ToUpper()
                            .EndsWith("ASSIGN")
                     )
                     .Select(p => p.Type)
                     .ToArray();

        /// <summary>
        ///     Maps language symbol's string value to it's token type.
        /// </summary>
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
        ///     Contains all characters that start an operator or symbol.
        /// </summary>
        public static readonly char[] SymbolicChars =
             SortedSymbolics
                 .Select(val => val[0])
                 .Distinct()
                 .Where(c => !char.IsLetter(c))
                 .ToArray();
    }
}