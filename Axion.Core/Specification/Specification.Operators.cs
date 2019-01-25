using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Specification {
    public partial class Spec {
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

        public static readonly Dictionary<string, OperatorProperties> Operators = new Dictionary<string, OperatorProperties> {
            { "++", new OperatorProperties(OpIncrement, InputSide.SomeOne, Associativity.RightToLeft, false, 45) },
            { "--", new OperatorProperties(OpDecrement, InputSide.SomeOne, Associativity.RightToLeft, false, 45) },
            { "~", new OperatorProperties(OpBitwiseNot, InputSide.Right,   Associativity.RightToLeft, false, 45) },

            { "*", new OperatorProperties(OpMultiply,     InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "**", new OperatorProperties(OpPower,       InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "/", new OperatorProperties(OpTrueDivide,   InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "//", new OperatorProperties(OpFloorDivide, InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "%", new OperatorProperties(OpRemainder,    InputSide.Both, Associativity.LeftToRight, false, 40) },

            { "+", new OperatorProperties(OpAdd,      InputSide.Both, Associativity.LeftToRight, false, 35) },
            { "-", new OperatorProperties(OpSubtract, InputSide.Both, Associativity.LeftToRight, false, 35) },

            { "<<", new OperatorProperties(OpLeftShift,  InputSide.Both, Associativity.LeftToRight, false, 30) },
            { ">>", new OperatorProperties(OpRightShift, InputSide.Both, Associativity.LeftToRight, false, 30) },

            { "<", new OperatorProperties(OpLessThan,            InputSide.Both, Associativity.LeftToRight, false, 25) },
            { "<=", new OperatorProperties(OpLessThanOrEqual,    InputSide.Both, Associativity.LeftToRight, false, 25) },
            { ">", new OperatorProperties(OpGreaterThan,         InputSide.Both, Associativity.LeftToRight, false, 25) },
            { ">=", new OperatorProperties(OpGreaterThanOrEqual, InputSide.Both, Associativity.LeftToRight, false, 25) },

            { "==", new OperatorProperties(OpEquals,    InputSide.Both, Associativity.LeftToRight, false, 20) },
            { "!=", new OperatorProperties(OpNotEquals, InputSide.Both, Associativity.LeftToRight, false, 20) },

            { "&", new OperatorProperties(OpBitwiseAnd,  InputSide.Both, Associativity.LeftToRight, false, 17) },
            { "^", new OperatorProperties(OpExclusiveOr, InputSide.Both, Associativity.LeftToRight, false, 16) },
            { "|", new OperatorProperties(OpBitwiseOr,   InputSide.Both, Associativity.LeftToRight, false, 15) },

            { "&&", new OperatorProperties(OpAnd, InputSide.Both, Associativity.LeftToRight, false, 11) },
            { "||", new OperatorProperties(OpOr,  InputSide.Both, Associativity.LeftToRight, false, 10) }
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

        public static readonly TokenType[] AugmentedAssignOperators = {
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

        public static readonly string[] SymbolicValues = Operators.Keys.Union(Symbols.Keys).OrderByDescending(val => val.Length).ToArray();
        public static readonly char[]   SymbolicChars  = SymbolicValues.Select(val => val[0]).ToArray();
    }
}