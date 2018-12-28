using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Specification {
    public partial class Spec {
        internal static readonly Dictionary<string, TokenType> Symbols = new Dictionary<string, TokenType> {
            { ".", TokenType.Dot },
            { "|>", TokenType.RightPipeline },
            { "<|", TokenType.LeftPipeline },
            { "+=", TokenType.AddAssign },
            { "-=", TokenType.SubtractAssign },
            { "**=", TokenType.PowerAssign },
            { "*=", TokenType.MultiplyAssign },
            { "/=", TokenType.FloorDivideAssign },
            { "//=", TokenType.TrueDivideAssign },
            { "%=", TokenType.RemainderAssign },
            { "??=", TokenType.NullCoalescingAssign },
            { "<<=", TokenType.BitLeftShiftAssign },
            { ">>=", TokenType.BitRightShiftAssign },
            { "&=", TokenType.BitAndAssign },
            { "|=", TokenType.BitOrAssign },
            { "^=", TokenType.BitExclusiveOrAssign },
            { "=", TokenType.Assign },
            { "(", TokenType.LeftParenthesis },
            { ")", TokenType.RightParenthesis },
            { "[", TokenType.LeftBracket },
            { "]", TokenType.RightBracket },
            { "{", TokenType.LeftBrace },
            { "}", TokenType.RightBrace },
            { ",", TokenType.Comma },
            { ":", TokenType.Colon },
            { ";", TokenType.Semicolon },
            { "=>", TokenType.RightArrow }
        };

        public static readonly Dictionary<string, OperatorProperties> Operators = new Dictionary<string, OperatorProperties> {
            { "++", new OperatorProperties(TokenType.OpIncrement, InputSide.SomeOne, Associativity.RightToLeft, false, 45) },
            { "--", new OperatorProperties(TokenType.OpDecrement, InputSide.SomeOne, Associativity.RightToLeft, false, 45) },
            { "~", new OperatorProperties(TokenType.OpBitwiseNot, InputSide.Right,   Associativity.RightToLeft, false, 45) },

            { "*", new OperatorProperties(TokenType.OpMultiply,     InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "**", new OperatorProperties(TokenType.OpPower,       InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "/", new OperatorProperties(TokenType.OpTrueDivide,   InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "//", new OperatorProperties(TokenType.OpFloorDivide, InputSide.Both, Associativity.LeftToRight, false, 40) },
            { "%", new OperatorProperties(TokenType.OpRemainder,    InputSide.Both, Associativity.LeftToRight, false, 40) },

            { "+", new OperatorProperties(TokenType.OpAdd,      InputSide.Both, Associativity.LeftToRight, false, 35) },
            { "-", new OperatorProperties(TokenType.OpSubtract, InputSide.Both, Associativity.LeftToRight, false, 35) },

            { "<<", new OperatorProperties(TokenType.OpLeftShift,  InputSide.Both, Associativity.LeftToRight, false, 30) },
            { ">>", new OperatorProperties(TokenType.OpRightShift, InputSide.Both, Associativity.LeftToRight, false, 30) },

            { "<", new OperatorProperties(TokenType.OpLessThan,            InputSide.Both, Associativity.LeftToRight, false, 25) },
            { "<=", new OperatorProperties(TokenType.OpLessThanOrEqual,    InputSide.Both, Associativity.LeftToRight, false, 25) },
            { ">", new OperatorProperties(TokenType.OpGreaterThan,         InputSide.Both, Associativity.LeftToRight, false, 25) },
            { ">=", new OperatorProperties(TokenType.OpGreaterThanOrEqual, InputSide.Both, Associativity.LeftToRight, false, 25) },

            { "==", new OperatorProperties(TokenType.OpEquals,    InputSide.Both, Associativity.LeftToRight, false, 20) },
            { "!=", new OperatorProperties(TokenType.OpNotEquals, InputSide.Both, Associativity.LeftToRight, false, 20) },

            { "&", new OperatorProperties(TokenType.OpBitwiseAnd,  InputSide.Both, Associativity.LeftToRight, false, 17) },
            { "^", new OperatorProperties(TokenType.OpExclusiveOr, InputSide.Both, Associativity.LeftToRight, false, 16) },
            { "|", new OperatorProperties(TokenType.OpBitwiseOr,   InputSide.Both, Associativity.LeftToRight, false, 15) },

            { "&&", new OperatorProperties(TokenType.OpAnd, InputSide.Both, Associativity.LeftToRight, false, 11) },
            { "||", new OperatorProperties(TokenType.OpOr,  InputSide.Both, Associativity.LeftToRight, false, 10) }
        };

        public static readonly TokenType[] ComparisonOperators = {
            TokenType.OpLessThan,
            TokenType.OpLessThanOrEqual,
            TokenType.OpGreaterThan,
            TokenType.OpGreaterThanOrEqual,
            TokenType.OpEquals,
            TokenType.OpNotEquals,

            TokenType.KeywordIs,
            TokenType.KeywordIsNot,
            TokenType.KeywordIn,
            TokenType.KeywordNotIn,

            TokenType.KeywordNot
        };

        public static readonly TokenType[] AugmentedAssignOperators = {
            TokenType.AddAssign,
            TokenType.SubtractAssign,
            TokenType.MultiplyAssign,
            TokenType.PowerAssign,
            TokenType.TrueDivideAssign,
            TokenType.FloorDivideAssign,
            TokenType.RemainderAssign,
            TokenType.NullCoalescingAssign,
            TokenType.BitAndAssign,
            TokenType.BitOrAssign,
            TokenType.BitExclusiveOrAssign,
            TokenType.BitLeftShiftAssign,
            TokenType.BitRightShiftAssign
        };

        public static readonly string[] SymbolicValues = Operators.Keys.Union(Symbols.Keys).OrderByDescending(val => val.Length).ToArray();
        public static readonly char[]   SymbolicChars  = SymbolicValues.Select(val => val[0]).ToArray();
    }
}