namespace Axion.Core.Processing.Errors {
    /// <summary>
    ///     Contains all error IDs that can happen while processing <see cref="SourceCode" />.
    /// </summary>
    public enum ErrorType {
        /// <summary>
        ///     No error is occurred.
        /// </summary>
        None,

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

        #region Mismatched pairs

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

        #endregion

        /// <summary>
        ///     Indicates a multiline comment
        ///     without closing block.
        /// </summary>
        UnclosedMultilineComment,

        /// <summary>
        ///     Indicates that string or character
        ///     literal uses invalid symbol to escape by '\'.
        /// </summary>
        InvalidEscapeSequence,

        /// <summary>
        ///     Indicates a string/char with unrecognized
        ///     Unicode character.
        /// </summary>
        IllegalUnicodeCharacter,

        /// <summary>
        ///     Indicates a string/char with truncated
        ///     Unicode symbol escape sequence.
        /// </summary>
        Truncated_uXXXX_Escape,

        #region String literal errors

        /// <summary>
        ///     Indicates a string with
        ///     missing ending quote.
        /// </summary>
        UnclosedString,

        InvalidPrefixInStringLiteral,

        UnescapedQuoteInStringLiteral,

        #endregion

        #region Character literal errors

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

        #endregion

        #region Number literal errors

        // Invalid number literals
        InvalidNumberLiteral,
        InvalidBinaryLiteral,
        InvalidOctalLiteral,
        InvalidHexadecimalLiteral,

        InvalidPostfixInNumberLiteral,

        /// <summary>
        ///     Indicates a number with more than one point.
        /// </summary>
        RepeatedDotInNumberLiteral,

        /// <summary>
        ///     Indicates a number without value
        ///     after '0radix' postfix.
        /// </summary>
        ExpectedNumberValueAfterNumberBaseSpecifier,

        /// <summary>
        ///     Indicates a number without value
        ///     after exponent, like '0.3e'.
        /// </summary>
        ExpectedNumberAfterExponentSign,

        /// <summary>
        ///     Indicates a number with postfix,
        ///     followed by some value.
        /// </summary>
        ExpectedEndOfNumberAfterPostfix,

        /// <summary>
        ///     Indicates a number with 'i' postfix,
        ///     but isn't followed with bit rate number (but it should be).
        /// </summary>
        ExpectedABitRateAfterNumberPostfix,

        /// <summary>
        ///     Indicates an integer number with invalid bit rate.
        /// </summary>
        InvalidIntegerNumberBitRate,

        /// <summary>
        ///     Indicates a floating point number with invalid bit rate.
        /// </summary>
        InvalidFloatNumberBitRate,

        #endregion

        TruncatedEscapeSequence,
        BadCharacterForIntegerValue,
        InvalidComplexNumberLiteral,
        ComplexLiteralTooLarge,
        InvalidXEscapeFormat
    }
}