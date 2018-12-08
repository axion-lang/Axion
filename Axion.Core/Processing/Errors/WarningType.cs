namespace Axion.Core.Processing.Errors {
    /// <summary>
    ///     Contains all warning IDs that can happen while processing <see cref="SourceCode" />.
    /// </summary>
    public enum WarningType {
        /// <summary>
        ///     No warning is occurred.
        /// </summary>
        None,

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
        RedundantExponentForZeroNumber
    }
}