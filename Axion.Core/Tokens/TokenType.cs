using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;Type&gt; of <see cref="Token" />.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenType {
        #region GENERATION_operators

        OpLeftParenthesis    = 40,
        OpRightParenthesis   = 41,
        OpLeftBracket        = 42,
        OpRightBracket       = 43,
        OpLeftBrace          = 44,
        OpRightBrace         = 45,
        OpDot                = 46,
        OpIncrement          = 47,
        OpDecrement          = 48,
        OpNot                = 49,
        OpBitwiseNot         = 50,
        OpMultiply           = 51,
        OpTrueDivide         = 52,
        OpRemainder          = 53,
        OpAdd                = 54,
        OpSubtract           = 55,
        OpLeftShift          = 56,
        OpRightShift         = 57,
        OpIn                 = 58,
        OpLessThan           = 59,
        OpLessThanOrEqual    = 60,
        OpGreaterThan        = 61,
        OpGreaterThanOrEqual = 62,
        OpEquals             = 63,
        OpNotEquals          = 64,
        OpBitwiseAnd         = 65,
        OpExclusiveOr        = 66,
        OpBitwiseOr          = 67,
        OpAnd                = 68,
        OpOr                 = 69,
        OpAssign             = 70,
        OpAddEqual           = 71,
        OpSubtractEqual      = 72,
        OpMultiplyEqual      = 73,
        OpTrueDivideEqual    = 74,
        OpRemainderEqual     = 75,
        OpLeftShiftEqual     = 76,
        OpRightShiftEqual    = 77,
        OpBitwiseAndEqual    = 78,
        OpBitwiseOrEqual     = 79,
        OpExclusiveOrEqual   = 80,
        OpRightArrow         = 81,
        OpComma              = 82,
        OpColon              = 83,
        OpSemicolon          = 84,

        #endregion GENERATION_operators

        Unknown,
        Invalid,

        Operator,
        Identifier,
        Keyword,

        CommentLiteral,
        CharLiteral,
        StringLiteral,
        IntegerLiteral,
        RealLiteral,
        FloatLiteral,

        Whitespace,
        Newline,
        Indent,
        Outdent,
        EndOfStream
    }
}