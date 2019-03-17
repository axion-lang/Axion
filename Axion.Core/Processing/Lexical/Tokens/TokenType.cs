using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Contains all types of tokens, available in language specification.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenType {
        None,
        Invalid,

        #region GENERATION_operators

        OpAdd,
        OpAnd,
        OpAs,
        OpBitwiseAnd,
        OpBitwiseNot,
        OpBitwiseOr,
        OpDecrement,
        OpEquals,
        OpExclusiveOr,
        OpFloorDivide,
        OpGreaterThan,
        OpGreaterThanOrEqual,
        OpIn,
        OpIncrement,
        OpIs,
        OpIsNot,
        OpLeftShift,
        OpLessThan,
        OpLessThanOrEqual,
        OpMultiply,
        OpNot,
        OpNotEquals,
        OpNotIn,
        OpOr,
        OpPower,
        OpRemainder,
        OpRightShift,
        OpSubtract,
        OpThreeWayCompare,
        OpTrueDivide,

        #endregion GENERATION_operators

        #region GENERATION_keywords

        KeywordAnyway,
        KeywordAssert,
        KeywordAsync,
        KeywordAwait,
        KeywordBreak,
        KeywordCase,
        KeywordCatch,
        KeywordClass,
        KeywordConst,
        KeywordContinue,
        KeywordDefault,
        KeywordDelete,
        KeywordDo,
        KeywordElse,
        KeywordElseIf,
        KeywordEnum,
        KeywordExtends,
        KeywordFalse,
        KeywordFn,
        KeywordFor,
        KeywordFrom,
        KeywordIf,
        KeywordMatch,
        KeywordMixin,
        KeywordModule,
        KeywordNew,
        KeywordNil,
        KeywordNoBreak,
        KeywordPass,
        KeywordRaise,
        KeywordReturn,
        KeywordSelf,
        KeywordStruct,
        KeywordTrue,
        KeywordTry,
        KeywordUnless,
        KeywordUse,
        KeywordVar,
        KeywordWhile,
        KeywordWith,
        KeywordWhen,
        KeywordYield,

        #endregion

        // symbols
        AddAssign,
        Assign,
        At,
        BitAndAssign,
        BitExclusiveOrAssign,
        BitLeftShiftAssign,
        BitOrAssign,
        BitRightShiftAssign,
        Colon,
        Comma,
        Dot,
        DoubleColon,
        FloorDivideAssign,
        LeftBrace,
        LeftBracket,
        LeftParenthesis,
        LeftPipeline,
        MultiplyAssign,
        NullCoalescingAssign,
        PowerAssign,
        Question,
        RemainderAssign,
        RightBrace,
        RightBracket,
        RightFatArrow,
        RightParenthesis,
        RightPipeline,
        Semicolon,
        SubtractAssign,
        TrueDivideAssign,

        // literals
        Identifier,
        Comment,
        Character,
        String,
        Number,

        // white
        Whitespace,
        Newline,
        Indent,
        Outdent,
        EndOfCode
    }
}