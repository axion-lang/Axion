using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

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

            // @formatter:off

            public static readonly Dictionary<string, string> BuiltInNames = new Dictionary<string, string> {
                { "Int8",         "sbyte" },
                { "UInt8",        "byte" },
                { "Int16",        "short" },
                { "UInt16",       "ushort" },
                { "Int32",        "int" },
                { "UInt32",       "uint" },
                { "Int64",        "long" },
                { "UInt64",       "ulong" },
                { "Float32",      "float" },
                { "Float64",      "double" },
                { "Float128",     "decimal" },
                { "Char",         "char" },
                { "Bool",         "bool" },
                { "Object",       "object" },
                { "self",         "this" }
            };

            public static readonly Dictionary<TokenType, string> BinaryOperators = new Dictionary<TokenType, string> {
                { OpPlus,               "+" },
                { OpMinus,              "-" },
                { OpTrueDivide,         "/" },
                { OpRemainder,          "%" },
                { OpMultiply,           "*" },

                { OpAnd,                "&&" },
                { OpOr,                 "||" },

                { OpBitAnd,             "&" },
                { OpBitOr,              "|" },
                { OpBitXor,             "^" },

                { OpEqualsEquals,       "==" },
                { OpNotEquals,          "!=" },

                { KeywordAs,            "as" },
                { OpIs,                 "is" },


                { OpLess,               "<" },
                { OpGreater,            ">" },
                { OpLessOrEqual,        "<=" },
                { OpGreaterOrEqual,     ">=" },
                { OpAssign,             "=" },
                { OpPlusAssign,         "+=" },
                { OpTrueDivideAssign,   "/=" },
                { OpRemainderAssign,    "%=" },
                { OpMultiplyAssign,     "*=" },
                { OpMinusAssign,        "-=" },
                { OpBitOrAssign,        "|=" },
                { OpBitAndAssign,       "&=" },
                { OpBitLShiftAssign,    "<<=" },
                { OpBitRShiftAssign,    ">>=" }
            };
            
            // @formatter:on
        }
    }
}