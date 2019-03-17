using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Utils;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        public static readonly Dictionary<string, OperatorProperties> Operators =
            new Dictionary<string, OperatorProperties> {
            { "++",  new OperatorProperties(OpIncrement, 15, InputSide.Unknown) },
            { "--",  new OperatorProperties(OpDecrement, 15, InputSide.Unknown) },
            
            { "**",  new OperatorProperties(OpPower, 14) },
            
            { "not", new OperatorProperties(OpNot,        13, InputSide.Left) },
            { "~",   new OperatorProperties(OpBitwiseNot, 13, InputSide.Left) },
            
            { "*",   new OperatorProperties(OpMultiply,    12) },
            { "/",   new OperatorProperties(OpTrueDivide,  12) },
            { "//",  new OperatorProperties(OpFloorDivide, 12) },
            { "%",   new OperatorProperties(OpRemainder,   12) },

            { "+",   new OperatorProperties(OpAdd,      11, InputSide.Unknown) },
            { "-",   new OperatorProperties(OpSubtract, 11, InputSide.Unknown) },

            { "<<",  new OperatorProperties(OpLeftShift,  10) },
            { ">>",  new OperatorProperties(OpRightShift, 10) },
            
            { "<=>", new OperatorProperties(OpThreeWayCompare, 9) },
            
            { "<",   new OperatorProperties(OpLessThan,           8) },
            { "<=",  new OperatorProperties(OpLessThanOrEqual,    8) },
            { ">",   new OperatorProperties(OpGreaterThan,        8) },
            { ">=",  new OperatorProperties(OpGreaterThanOrEqual, 8) },

            { "==",  new OperatorProperties(OpEquals,    7) },
            { "!=",  new OperatorProperties(OpNotEquals, 7) },
            
            { "&",   new OperatorProperties(OpBitwiseAnd,  6) },
            { "^",   new OperatorProperties(OpExclusiveOr, 5) },
            { "|",   new OperatorProperties(OpBitwiseOr,   4) },
            
            // + is not, not in
            { "is",  new OperatorProperties(OpIs, 3) },
            { "as",  new OperatorProperties(OpAs, 3) },
            { "in",  new OperatorProperties(OpIn, 2) },
            
            { "and", new OperatorProperties(OpAnd, 1) },
            { "or",  new OperatorProperties(OpOr,  0) }
        };
        
        public static readonly Dictionary<TokenType, string> OperatorTypes =
            new Dictionary<TokenType, string> {
            { OpIncrement,          "++" },
            { OpDecrement,          "--" },
            
            { OpPower,              "**" },
            
            { OpNot,                "not" },
            { OpBitwiseNot,         "~" },
            
            { OpMultiply,           "*" },
            { OpTrueDivide,         "/" },
            { OpFloorDivide,        "//" },
            { OpRemainder,          "%" },

            { OpAdd,                "+" },
            { OpSubtract,           "-" },

            { OpLeftShift,          "<<" },
            { OpRightShift,         ">>" },
            
            { OpThreeWayCompare,    "<=>" },
            
            { OpLessThan,           "<" },
            { OpLessThanOrEqual,    "<=" },
            { OpGreaterThan,        ">" },
            { OpGreaterThanOrEqual, ">=" },

            { OpEquals,             "==" },
            { OpNotEquals,          "!=" },
            
            { OpBitwiseAnd,         "&" },
            { OpExclusiveOr,        "^" },
            { OpBitwiseOr,          "|" },
            
            // + is not, not in
            { OpIs,                 "is" },
            { OpAs,                 "as" },
            { OpIn,                 "in" },
            
            { OpAnd,                "and" },
            { OpOr,                 "or" }
        };
        
        public static readonly OperatorProperties OperatorIsNot =
            new OperatorProperties(OpIsNot, 3);
        
        public static readonly OperatorProperties OperatorNotIn =
            new OperatorProperties(OpNotIn, 2);
        
        public static readonly TokenType[] FactorOperators = {
            OpAdd,
            OpSubtract,
            OpBitwiseNot,
            OpNot,
            OpIncrement,
            OpDecrement
        };

        public static readonly TokenType[] CompoundAssignOperators = {
            AddAssign,
            SubtractAssign,
            MultiplyAssign,
            PowerAssign,
            TrueDivideAssign,
            FloorDivideAssign,
            RemainderAssign,
            NullCoalescingAssign,
            BitAndAssign,
            BitOrAssign,
            BitExclusiveOrAssign,
            BitLeftShiftAssign,
            BitRightShiftAssign
        };

        internal static readonly Map<string, TokenType> Symbols =
            new Map<string, TokenType> {
            { "=",   Assign },
            { "+=",  AddAssign },
            { "-=",  SubtractAssign },
            { "**=", PowerAssign },
            { "*=",  MultiplyAssign },
            { "/=",  FloorDivideAssign },
            { "//=", TrueDivideAssign },
            { "%=",  RemainderAssign },
            { "?=",  NullCoalescingAssign },
            { "<<=", BitLeftShiftAssign },
            { ">>=", BitRightShiftAssign },
            { "&=",  BitAndAssign },
            { "|=",  BitOrAssign },
            { "^=",  BitExclusiveOrAssign },
            { ".",   Dot },
            { "|>",  RightPipeline },
            { "<|",  LeftPipeline },
            { "=>",  RightFatArrow },
            { "@",   At },
            { "?",   Question },
            { "::",  DoubleColon },

            { "(",   LeftParenthesis },
            { ")",   RightParenthesis },
            { "[",   LeftBracket },
            { "]",   RightBracket },
            { "{",   LeftBrace },
            { "}",   RightBrace },
            { ",",   Comma },
            { ":",   Colon },
            { ";",   Semicolon }
        };

        /// <summary>
        ///     Symbolic = Operators + Symbols
        /// </summary>
        public static readonly string[] SortedSymbolicValues
            = Operators.Keys.Union(Symbols.Forward.Keys).OrderByDescending(val => val.Length).ToArray();
        
        public static readonly char[] SymbolicChars
            = SortedSymbolicValues.Select(val => val[0]).ToArray();
    }
}