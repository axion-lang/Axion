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

        OpIncrement,
        OpDecrement,
        OpBitwiseNot,
        OpMultiply,
        OpTrueDivide,
        OpRemainder,
        OpAdd,
        OpSubtract,
        OpLeftShift,
        OpRightShift,
        OpLessThan,
        OpLessThanOrEqual,
        OpGreaterThan,
        OpGreaterThanOrEqual,
        OpEquals,
        OpNotEquals,
        OpBitwiseAnd,
        OpExclusiveOr,
        OpBitwiseOr,
        OpFloorDivide,
        OpPower,

        #endregion GENERATION_operators

        #region GENERATION_keywords

        KeywordAnd,
        KeywordAnyway,
        KeywordAs,
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
        KeywordElseIf,
        KeywordElse,
        KeywordEnum,
        KeywordExtends,
        KeywordFalse,
        KeywordFn,
        KeywordFor,
        KeywordFrom,
        KeywordIf,
        KeywordNotIn,
        KeywordIn,

//        KeywordInner,
        KeywordIs,
        KeywordIsNot,
        KeywordMatch,
        KeywordMixin,
        KeywordNew,
        KeywordModule,
        KeywordNoBreak,
        KeywordNot,
        KeywordNil,
        KeywordOr,
        KeywordPass,

//        KeywordPrivate,
//        KeywordPublic,
//        KeywordReact,
//        KeywordReadonly,
        KeywordReturn,
        KeywordSelf,

//        KeywordSingleton,
//        KeywordStatic,
        KeywordStruct,
        KeywordRaise,
        KeywordTrue,
        KeywordTry,
        KeywordUnless,
        KeywordUse,
        KeywordVar,
        KeywordWhile,
        KeywordWith,
        KeywordYield,

        #endregion

        // symbols

        Assign,
        AddAssign,
        SubtractAssign,
        MultiplyAssign,
        TrueDivideAssign,
        RemainderAssign,

        BitLeftShiftAssign,
        BitRightShiftAssign,
        BitAndAssign,
        BitOrAssign,
        BitExclusiveOrAssign,

        PowerAssign,
        FloorDivideAssign,
        NullCoalescingAssign,

        Dot,
        RightPipeline,
        LeftPipeline,
        RightFatArrow,
        At,
        Question,
        DoubleColon,

        LeftParenthesis,
        RightParenthesis,
        LeftBracket,
        RightBracket,
        LeftBrace,
        RightBrace,

        Comma,
        Colon,
        Semicolon,

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