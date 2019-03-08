using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        public static readonly Dictionary<string, OperatorProperties> Operators =
            new Dictionary<string, OperatorProperties> {
            { "++",  new OperatorProperties(OpIncrement, false, 15) },
            { "--",  new OperatorProperties(OpDecrement, false, 15) },
            
            { "**",  new OperatorProperties(OpPower, false, 14) },
            
            { "not", new OperatorProperties(KeywordNot,   false, 13) },
            { "~",   new OperatorProperties(OpBitwiseNot, false, 13) },
            
            { "*",   new OperatorProperties(OpMultiply,    false, 12) },
            { "/",   new OperatorProperties(OpTrueDivide,  false, 12) },
            { "//",  new OperatorProperties(OpFloorDivide, false, 12) },
            { "%",   new OperatorProperties(OpRemainder,   false, 12) },

            { "+",   new OperatorProperties(OpAdd,      false, 11) },
            { "-",   new OperatorProperties(OpSubtract, false, 11) },

            { "<<",  new OperatorProperties(OpLeftShift,  false, 10) },
            { ">>",  new OperatorProperties(OpRightShift, false, 10) },
            
            { "<=>", new OperatorProperties(OpThreeWayCompare, false, 9) },
            
            { "<",   new OperatorProperties(OpLessThan,           false, 8) },
            { "<=",  new OperatorProperties(OpLessThanOrEqual,    false, 8) },
            { ">",   new OperatorProperties(OpGreaterThan,        false, 8) },
            { ">=",  new OperatorProperties(OpGreaterThanOrEqual, false, 8) },

            { "==",  new OperatorProperties(OpEquals,    false, 7) },
            { "!=",  new OperatorProperties(OpNotEquals, false, 7) },
            
            { "&",   new OperatorProperties(OpBitwiseAnd,  false, 6) },
            { "^",   new OperatorProperties(OpExclusiveOr, false, 5) },
            { "|",   new OperatorProperties(OpBitwiseOr,   false, 4) },
            
            // + is not, not in
            { "is",  new OperatorProperties(KeywordIs, false, 3) },
            { "in",  new OperatorProperties(KeywordIn, false, 2) },
            
            { "and", new OperatorProperties(KeywordAnd, false, 1) },
            { "or",  new OperatorProperties(KeywordOr,  false, 0) }
        };
        
        public static readonly OperatorProperties OperatorIsNot =
            new OperatorProperties(KeywordIsNot, false, 3);
        
        public static readonly OperatorProperties OperatorNotIn =
            new OperatorProperties(KeywordNotIn, false, 2);
        
        public static readonly TokenType[] FactorOperators = {
            OpAdd,
            OpSubtract,
            OpBitwiseNot,
            KeywordNot,
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

        internal static readonly Dictionary<string, TokenType> Symbols = new Dictionary<string, TokenType> {
            { "=", Assign },
            { "+=", AddAssign },
            { "-=", SubtractAssign },
            { "**=", PowerAssign },
            { "*=", MultiplyAssign },
            { "/=", FloorDivideAssign },
            { "//=", TrueDivideAssign },
            { "%=", RemainderAssign },
            { "?=", NullCoalescingAssign },
            { "<<=", BitLeftShiftAssign },
            { ">>=", BitRightShiftAssign },
            { "&=", BitAndAssign },
            { "|=", BitOrAssign },
            { "^=", BitExclusiveOrAssign },
            { ".", Dot },
            { "|>", RightPipeline },
            { "<|", LeftPipeline },
            { "=>", RightFatArrow },
            { "@", At },
            { "?", Question },
            { "::", DoubleColon },

            { "(", LeftParenthesis },
            { ")", RightParenthesis },
            { "[", LeftBracket },
            { "]", RightBracket },
            { "{", LeftBrace },
            { "}", RightBrace },
            { ",", Comma },
            { ":", Colon },
            { ";", Semicolon }
        };

        public static readonly string[] SymbolicValues = Operators.Keys.Union(Symbols.Keys).OrderByDescending(val => val.Length).ToArray();
        public static readonly char[]   SymbolicChars  = SymbolicValues.Select(val => val[0]).ToArray();
    }
}