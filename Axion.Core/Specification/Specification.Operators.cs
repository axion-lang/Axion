using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
        public static readonly Dictionary<string, OperatorProperties> Operators = new Dictionary<string, OperatorProperties> {
            { "++", new OperatorProperties(OpIncrement,  false, 40) },
            { "--", new OperatorProperties(OpDecrement,  false, 40) },
            { "~", new OperatorProperties(OpBitwiseNot,  false, 40) },

            { "*", new OperatorProperties(OpMultiply,      false, 35) },
            { "**", new OperatorProperties(OpPower,        false, 35) },
            { "/", new OperatorProperties(OpTrueDivide,    false, 35) },
            { "//", new OperatorProperties(OpFloorDivide,  false, 35) },
            { "%", new OperatorProperties(OpRemainder,     false, 35) },

            { "+", new OperatorProperties(OpAdd,       false, 30) },
            { "-", new OperatorProperties(OpSubtract,  false, 30) },

            { "<<", new OperatorProperties(OpLeftShift,   false, 25) },
            { ">>", new OperatorProperties(OpRightShift,  false, 25) },

            { "<", new OperatorProperties(OpLessThan,             false, 20) },
            { "<=", new OperatorProperties(OpLessThanOrEqual,     false, 20) },
            { ">", new OperatorProperties(OpGreaterThan,          false, 20) },
            { ">=", new OperatorProperties(OpGreaterThanOrEqual,  false, 20) },

            { "==", new OperatorProperties(OpEquals,     false, 15) },
            { "!=", new OperatorProperties(OpNotEquals,  false, 15) },

            { "&", new OperatorProperties(OpBitwiseAnd,   false, 12) },
            { "^", new OperatorProperties(OpExclusiveOr,  false, 11) },
            { "|", new OperatorProperties(OpBitwiseOr,    false, 10) }
        };

        public static readonly TokenType[] BooleanOperators = {
            OpLessThan,
            OpLessThanOrEqual,
            OpGreaterThan,
            OpGreaterThanOrEqual,
            OpEquals,
            OpNotEquals,
            KeywordAnd,
            KeywordOr
        };

        public static readonly TokenType[] ComparisonOperators = {
            OpLessThan,
            OpLessThanOrEqual,
            OpGreaterThan,
            OpGreaterThanOrEqual,
            OpEquals,
            OpNotEquals,

            KeywordIs,
            KeywordIsNot,
            KeywordIn,
            KeywordNotIn,

            KeywordNot
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
            // TODO: <=> operator from c++20
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