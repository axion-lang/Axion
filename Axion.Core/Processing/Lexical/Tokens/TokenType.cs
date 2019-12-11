using System;
using System.Linq;
using Axion.Core.Specification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Contains all types of tokens
    ///     available in language specification.
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
        OpOf,
        OpDot,
        Op2Dot,
        Op3Dot,
        OpAnd,
        OpOr,
        KeywordAs,
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
        OpBitLShiftAssign,
        OpBitRShiftAssign,
        OpPlusAssign,
        OpMinusAssign,
        OpMultiplyAssign,
        OpRemainderAssign,
        OpFloorDivideAssign,
        OpTrueDivideAssign,
        OpPowerAssign,
        OpQuestionAssign,
        OpAssign,

        #endregion

        #region GEN_keywords

        KeywordAwait,
        KeywordBreak,
        KeywordClass,
        KeywordContinue,
        KeywordElse,
        KeywordElif,
        KeywordFalse,
        KeywordFn,
        KeywordFor,
        KeywordFrom,
        KeywordIf,
        KeywordModule,
        KeywordNil,
        KeywordNoBreak,
        KeywordPass,
        KeywordReturn,
        KeywordTrue,
        KeywordUnless,
        KeywordLet,
        KeywordWhile,
        KeywordYield,
        CustomKeyword,

        #endregion

        #region GEN_symbols

        Question,
        LeftArrow,
        RightArrow,
        RightPipeline,
        At,
        Comma,
        Semicolon,
        Colon,
        Dollar,

        // brackets
        OpenBrace,
        DoubleOpenBrace,
        OpenBracket,
        OpenParenthesis,
        CloseBrace,
        DoubleCloseBrace,
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

        internal static string GetValue(this TokenType type) {
            string value = Spec.Keywords.FirstOrDefault(kvp => kvp.Value == type).Key;
            if (value != null) {
                return value;
            }

            value = Spec.Operators.FirstOrDefault(kvp => kvp.Value.Item1 == type).Key;
            if (value != null) {
                return value;
            }

            value = Spec.Punctuation.FirstOrDefault(kvp => kvp.Value == type).Key;
            if (value != null) {
                return value;
            }

            return type.ToString("G");
        }
    }
}