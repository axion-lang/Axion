using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Axion.Specification.TokenType;

namespace Axion.Specification;

/// <summary>
///     Contains all types of tokens
///     available in language specification.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TokenType {
    None,
    Invalid,

    #region

    KeywordAwait,
    KeywordBreak,
    KeywordClass,
    KeywordContinue,
    KeywordElse,
    KeywordElif,
    KeywordFalse,
    KeywordFn,
    KeywordFor,
    KeywordIf,
    KeywordImport,
    KeywordMacro,
    KeywordModule,
    KeywordNil,
    KeywordPass,
    KeywordReturn,
    KeywordTrue,
    KeywordUnless,
    KeywordLet,
    KeywordWhile,
    KeywordYield,
    CustomKeyword,

    #endregion

    #region

    Ampersand,
    And,
    Caret,
    Dot,
    DoubleDot,
    DoubleEquals,
    DoubleLeftAngle,
    DoubleMinus,
    DoublePlus,
    DoubleQuestion,
    DoubleRightAngle,
    DoubleSlash,
    DoubleStar,
    ExclamationEquals,
    In,
    Is,
    LeftAngle,
    LeftAngleEquals,
    LeftRightArrow,
    LeftRightFatArrow,
    Minus,
    Not,
    Of,
    Or,
    Percent,
    Pipe,
    PipeRightAngle,
    Plus,
    RightAngle,
    RightAngleEquals,
    Slash,
    Star,
    Tilde,
    TripleDot,

    AmpersandEquals,
    CaretEquals,
    DoubleLeftAngleEquals,
    DoubleRightAngleEquals,
    DoubleSlashEquals,
    DoubleStarEquals,
    EqualsSign,
    MinusEquals,
    PercentEquals,
    PipeEquals,
    PlusEquals,
    QuestionEquals,
    SlashEquals,
    StarEquals,


    At,
    Colon,
    Comma,
    Dollar,
    LeftArrow,
    Question,
    RightArrow,
    Semicolon,

    // brackets
    CloseBrace,
    CloseBracket,
    CloseParenthesis,
    DoubleCloseBrace,
    DoubleOpenBrace,
    OpenBrace,
    OpenBracket,
    OpenParenthesis,

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
    public static bool IsOpenBracket(this TokenType type) {
        return type is OpenParenthesis
            or OpenBracket
            or OpenBrace
            or DoubleOpenBrace;
    }

    public static bool IsCloseBracket(this TokenType type) {
        return type is CloseParenthesis
            or CloseBracket
            or CloseBrace
            or DoubleCloseBrace;
    }

    public static TokenType GetMatchingBracket(this TokenType type) {
        return type switch {
            CloseBrace       => OpenBrace,
            DoubleCloseBrace => DoubleOpenBrace,
            CloseBracket     => OpenBracket,
            CloseParenthesis => OpenParenthesis,
            OpenBrace        => CloseBrace,
            DoubleOpenBrace  => DoubleCloseBrace,
            OpenBracket      => CloseBracket,
            OpenParenthesis  => CloseParenthesis,
            _ => throw new InvalidOperationException(
                "Internal error: Cannot return matching bracket for non-bracket token type."
            )
        };
    }

    public static string GetValue(this TokenType type) {
        try {
            return Spec.Keywords.First(kvp => kvp.Value == type).Key;
        }
        catch {
            // ignored
        }
        try {
            return Spec.Operators.First(kvp => kvp.Value.Type == type).Key;
        }
        catch {
            // ignored
        }
        try {
            return Spec.Punctuation.First(kvp => kvp.Value == type).Key;
        }
        catch {
            // ignored
        }
        return type.ToString("G");
    }
}
