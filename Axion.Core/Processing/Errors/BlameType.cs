namespace Axion.Core.Processing.Errors {
    /// <summary>
    ///     Contains all error IDs that can
    ///     happen while processing source.
    /// </summary>
    public enum BlameType {
        #region ERRORS BY DEFAULT

        /// <summary>
        ///     Indicates an operator that isn't
        ///     declared in language specification.
        /// </summary>
        InvalidOperator,

        /// <summary>
        ///     Indicates a char that isn't
        ///     declared in language specification.
        /// </summary>
        InvalidCharacter,

        #region Mismatched pairs

        /// <summary>
        ///     Indicates a '(' in code
        ///     without matching ')'.
        /// </summary>
        MismatchedParenthesis,

        /// <summary>
        ///     Indicates a '[' in code
        ///     without matching ']'.
        /// </summary>
        MismatchedBracket,

        /// <summary>
        ///     Indicates a '{' in code
        ///     without matching '}'.
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
        ///     literal uses invalid character escaped by '\'.
        /// </summary>
        InvalidEscapeSequence,

        /// <summary>
        ///     Indicates a string/char with unrecognized
        ///     Unicode character.
        /// </summary>
        IllegalUnicodeCharacter,
        InvalidXEscapeFormat,
        TruncatedEscapeSequence,

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

        BadCharacterForIntegerValue,
        InvalidComplexNumberLiteral,
        ComplexLiteralTooLarge,

        #region High-level syntax errors

        BreakIsOutsideLoop,
        ContinueIsOutsideLoop,
        ContinueNotSupportedInsideFinally,
        MisplacedReturn,
        MisplacedYield,
        InvalidExpressionToDelete,
        DuplicatedParameterNameInFunctionDefinition,
        ExpectedIndentation,
        UnexpectedIndentation,
        InvalidIndentation,
        DefaultCatchMustBeLast,
        UnexpectedEndOfCode,
        DuplicatedNamedArgument,
        InvalidSyntax,
        ExpectedDefaultParameterValue,
        CannotUseAccessModifierOutsideClass,
        AsyncModifierIsInapplicableToThatStatement,
        ExpectedBlockDeclaration,
        ConstantValueExpected,
        InvalidTypeNameExpression,
        DecoratorCanOnlyBeANameWithOptionalArguments,

        #endregion

        #endregion

        #region WARNINGS BY DEFAULT

        /// <summary>
        ///     Indicates that input script uses
        ///     mixed indentation (spaces mixed with tabs).
        /// </summary>
        InconsistentIndentation,

        /// <summary>
        ///     Indicates that string literal has
        ///     prefix that repeated more than 1 time.
        /// </summary>
        DuplicatedStringPrefix,

        /// <summary>
        ///     Indicates that string has format prefix (f),
        ///     but does not have any interpolated piece inside.
        /// </summary>
        RedundantStringFormatPrefix,

        /// <summary>
        ///     Indicates that string is empty,
        ///     and has prefixes, whose are useless
        /// </summary>
        RedundantPrefixesForEmptyString,

        /// <summary>
        ///     Indicates that 0 number is followed
        ///     by exponent, that's meaningless.
        /// </summary>
        RedundantExponentForZeroNumber,

        #region High-level syntax warnings

        RedundantEmptyUseStatement,
        DoubleNegationIsMeaningless,
        RedundantColonWithBraces,
        RedundantEmptyListOfTypeArguments,

        #endregion

        #endregion
    }
}