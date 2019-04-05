using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;

// ReSharper disable once CheckNamespace
namespace Axion.Core.Specification {
    public partial class Spec {
        public class CSharp {
            public static readonly Dictionary<TokenType, string> BinaryOperators =
                new Dictionary<TokenType, string> {
                    { TokenType.OpPlus,                "+"  },
                    { TokenType.OpMinus,               "-"  },
                    { TokenType.OpTrueDivide,          "/"  },
                    { TokenType.OpRemainder,           "%"  },
                    { TokenType.OpMultiply,            "*"  },
                
                    { TokenType.OpAnd,                 "&&" },
                    { TokenType.OpOr,                  "||" },
                
                    { TokenType.OpBitAnd,              "&"  },
                    { TokenType.OpBitOr,               "|"  },
                    { TokenType.OpBitXor,              "^"  },

                    { TokenType.OpEqualsEquals,        "==" },
                    { TokenType.OpNotEquals,           "!=" },
                
                    { TokenType.OpAs,                  "as" },
                    { TokenType.OpIs,                  "is" },

                
                    { TokenType.OpLess,                "<"  },
                    { TokenType.OpGreater,             ">"  },
                    { TokenType.OpLessOrEqual,         "<=" },
                    { TokenType.OpGreaterOrEqual,      ">=" },
                    { TokenType.OpAssign,              "="},
                    { TokenType.OpPlusAssign,          "+="  },
                    { TokenType.OpTrueDivideAssign,    "/="  },
                    { TokenType.OpRemainderAssign,     "%="  },
                    { TokenType.OpMultiplyAssign,      "*="  },
                    { TokenType.OpMinusAssign,         "-="  },
                    { TokenType.OpBitOrAssign,         "|="  },
                    { TokenType.OpBitAndAssign,        "&="  },
                    { TokenType.OpBitLeftShiftAssign,  "<<=" },
                    { TokenType.OpBitRightShiftAssign, ">>=" }
                };
        }
    }
}