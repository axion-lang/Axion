using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Axion.Core.Specification {
    public partial class Spec {
        public class CSharp {
            public static readonly Assembly[] DefaultImports = {
                typeof(Enumerable).Assembly,
                typeof(BigInteger).Assembly
            };

            public static readonly string[] AccessModifiers = {
                "private",
                "internal",
                "protected",
                "public"
            };
            
            public static readonly Dictionary<string, string> BuiltInNames = new Dictionary<string, string> {
                { "Int8",        "sbyte" },
                { "UInt8",       "byte" },
                { "Int16",       "short" },
                { "UInt16",      "ushort" },
                { "Int32",       "int" },
                { "UInt32",      "uint" },
                { "Int64",       "long" },
                { "UInt64",      "ulong" },
                { "Float32",     "float" },
                { "Float64",     "double" },
                { "Float128",    "decimal" },
                { "Char",        "char" },
                { "Bool",        "bool" },
                { "Object",      "object" },
                { "self",        "this" }
            };
            
            public static readonly Dictionary<TokenType, string> BinaryOperators = new Dictionary<TokenType, string> {
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
                { TokenType.OpAssign,              "="  },
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