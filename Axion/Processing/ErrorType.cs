namespace Axion.Processing {
    /// <summary>
    ///     Contains all error IDs that can happen while processing <see cref="SourceCode" />.
    /// </summary>
    internal enum ErrorType {
        // unfinished expression
        UnclosedMultilineComment,
        UnclosedString,

        // mismatched pairs
        MismatchedParenthesis,
        MismatchedBracket,
        MismatchedBrace,

        // Invalid number literals
        InvalidIntegerLiteral,
        InvalidFloatLiteral,
        InvalidBinaryLiteral,
        InvalidOctalLiteral,
        InvalidHexadecimalLiteral,

        InvalidOperator,
        InvalidSymbol,

        InvalidEscapeSequence,
        CharacterLiteralTooLong,

        // ************************** \\
        // ******** WARNINGS ******** \\
        // ************************** \\
        WarnInconsistentIndentation
    }
}