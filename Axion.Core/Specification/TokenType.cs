using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        KeywordAs,
        KeywordIs,
        KeywordIsNot,
        KeywordNot,
        KeywordIn,
        KeywordNotIn,
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

        KeywordAll,
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
            return type == TokenType.OpenParenthesis
                   || type == TokenType.OpenBracket
                   || type == TokenType.OpenBrace;
        }

        internal static bool IsCloseBracket(this TokenType type) {
            return type == TokenType.CloseParenthesis
                   || type == TokenType.CloseBracket
                   || type == TokenType.CloseBrace;
        }

        internal static TokenType GetMatchingBracket(this TokenType type) {
            switch (type) {
                // open : close
                case TokenType.OpenParenthesis:
                    return TokenType.CloseParenthesis;
                case TokenType.OpenBracket:
                    return TokenType.CloseBracket;
                case TokenType.OpenBrace:
                    return TokenType.CloseBrace;
                // close : open
                case TokenType.CloseParenthesis:
                    return TokenType.OpenParenthesis;
                case TokenType.CloseBracket:
                    return TokenType.OpenBracket;
                case TokenType.CloseBrace:
                    return TokenType.OpenBrace;
                // should never be thrown
                default:
                    throw new Exception(
                        "Internal error: Cannot return matching bracket for non-bracket token type."
                    );
            }
        }
    }
}