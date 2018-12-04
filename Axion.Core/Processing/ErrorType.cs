namespace Axion.Core.Processing {
    /// <summary>
    ///     Contains all error IDs that can happen while processing <see cref="SourceCode" />.
    /// </summary>
    public enum ErrorType {
        /// <summary>
        ///     No error is occurred.
        /// </summary>
        None,

        /// <summary>
        ///     Indicates a '(' symbol in
        ///     code without matching ')'.
        /// </summary>
        MismatchedParenthesis,

        /// <summary>
        ///     Indicates a '[' symbol in
        ///     code without matching ']'.
        /// </summary>
        MismatchedBracket,

        /// <summary>
        ///     Indicates a '{' symbol in
        ///     code without matching '}'.
        /// </summary>
        MismatchedBrace,

        /// <summary>
        ///     Indicates a multiline comment
        ///     that went through end of input.
        /// </summary>
        UnclosedMultilineComment,

        /// <summary>
        ///     Indicates a string with
        ///     missing ending quote.
        /// </summary>
        UnclosedString,

        InvalidPrefixInStringLiteral,

        UnescapedQuoteInStringLiteral,

        /// <summary>
        ///     Indicates the character literal with
        ///     missing ending quote.
        /// </summary>
        UnclosedCharacterLiteral,

        /// <summary>
        ///     Indicates a character literal that
        ///     exceeds maximal allowed character literal length.
        /// </summary>
        CharacterLiteralTooLong,

        /// <summary>
        ///     Indicates that character literal
        ///     length is 0, that is prohibited.
        /// </summary>
        EmptyCharacterLiteral,

        // Invalid number literals
        InvalidIntegerLiteral,
        InvalidFloatLiteral,
        InvalidValueAfterExponent,
        InvalidPostfixInNumberLiteral,
        ExpectedABitRateAfterNumberPostfix,
        InvalidIntegerNumberBitRate,
        RepeatedDotInNumberLiteral,
        RepeatedLongPostfixInNumberLiteral,
        ShouldHaveNoValueAfterNumberLongPostfix,
        ShouldHaveNoValueAfterNumberImaginaryPostfix,
        InvalidBinaryLiteral,
        InvalidOctalLiteral,
        InvalidHexadecimalLiteral,

        ExpectedEndOfNumberAfterExponent,
        ExpectedDigitAfterNumberBaseSpecifier,
        ExpectedDigitAfterNumberExponent,
        ExpectedEndOfNumberAfterPostfix,

        RepeatedPostfixInNumberLiteral,
        RepeatedImaginarySignInNumberLiteral,
        InvalidFloatNumberBitRate,

        /// <summary>
        ///     Indicates an operator that isn't
        ///     declared in language specification.
        /// </summary>
        InvalidOperator,

        /// <summary>
        ///     Indicates a symbol that isn't
        ///     declared in language specification.
        /// </summary>
        InvalidSymbol,

        /// <summary>
        ///     Indicates that string or character
        ///     literal uses invalid symbol to escape by '\'.
        /// </summary>
        InvalidEscapeSequence,
        IllegalUnicodeCharacter,
        Truncated_uXXXX_Escape
    }
}