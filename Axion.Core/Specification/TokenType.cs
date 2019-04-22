using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Specification {
    /// <summary>
    ///     Contains all types of tokens, available in language specification.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenType {
        None,
        Invalid,

        #region GEN_operators

        OpBitAnd,
        OpBitNot,
        OpBitOr,
        OpBitXor,
        OpBitLeftShift,
        OpBitRightShift,
        OpAnd,
        OpOr,
        OpAs,
        OpIs,
        OpIsNot,
        OpNot,
        OpIn,
        OpNotIn,
        OpEqualsEquals,
        OpNotEquals,
        OpGreater,
        OpGreaterOrEqual,
        OpLess,
        OpLessOrEqual,
        OpPlus,
        OpMinus,
        OpIncrement,
        OpDecrement,
        OpMultiply,
        OpPower,
        OpTrueDivide,
        OpFloorDivide,
        OpRemainder,
        Op3WayCompare,
        Op2Question,

        // assignment marks
        OpBitAndAssign,
        OpBitOrAssign,
        OpBitXorAssign,
        OpBitLeftShiftAssign,
        OpBitRightShiftAssign,
        OpPlusAssign,
        OpMinusAssign,
        OpMultiplyAssign,
        OpRemainderAssign,
        OpFloorDivideAssign,
        OpTrueDivideAssign,
        OpPowerAssign,
        OpNullCoalescingAssign,
        OpAssign,

        #endregion

        #region GEN_keywords

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
        KeywordStruct,
        KeywordTrue,
        KeywordTry,
        KeywordUnless,
        KeywordUse,
        KeywordLet,
        KeywordWhile,
        KeywordWith,
        KeywordWhen,
        KeywordYield,

        #endregion

        #region GEN_symbols

        Question,
        RightFatArrow,
        LeftPipeline,
        RightPipeline,
        At,
        Dot,
        Comma,
        Semicolon,
        Colon,
        ColonColon,

        // brackets
        OpenBrace,
        OpenBracket,
        OpenParenthesis,
        CloseBrace,
        CloseBracket,
        CloseParenthesis,

        #endregion

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
        End
    }

    public static class TokenTypeExtensions {
        internal static bool IsOpenBracket(this TokenType type) {
            return type == OpenParenthesis
                   || type == OpenBracket
                   || type == OpenBrace;
        }

        internal static bool IsCloseBracket(this TokenType type) {
            return type == CloseParenthesis
                   || type == CloseBracket
                   || type == CloseBrace;
        }

        internal static TokenType GetMatchingBracket(this TokenType type) {
            switch (type) {
                // open : close
                case OpenParenthesis:
                    return CloseParenthesis;
                case OpenBracket:
                    return CloseBracket;
                case OpenBrace:
                    return CloseBrace;
                // close : open
                case CloseParenthesis:
                    return OpenParenthesis;
                case CloseBracket:
                    return OpenBracket;
                case CloseBrace:
                    return OpenBrace;
                // should never be thrown
                default:
                    throw new Exception(
                        "Internal error: Cannot return matching bracket for non-bracket token type."
                    );
            }
        }
    }
}