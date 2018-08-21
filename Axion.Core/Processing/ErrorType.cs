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
        ///     Indicates the multiline comment
        ///     that went through end of input.
        /// </summary>
        UnclosedMultilineComment,

        /// <summary>
        ///     Indicates the string with
        ///     missing ending quote.
        /// </summary>
        UnclosedString,

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

        // Invalid number literals
        InvalidIntegerLiteral,
        InvalidFloatLiteral,
        InvalidBinaryLiteral,
        InvalidOctalLiteral,
        InvalidHexadecimalLiteral,

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

        // ************************** \\
        // ******** WARNINGS ******** \\
        // ************************** \\

        /// <summary>
        ///     Indicates that input script uses
        ///     mixed indentation (spaces mixed with tabs).
        /// </summary>
        WarnInconsistentIndentation
    }
}