namespace Axion.Core {
    /// <summary>
    ///     Determines how compiler must
    ///     process source unit.
    /// </summary>
    public enum Mode {
        Default = Reduction,

        /// <summary>
        ///     Perform lexical analysis on source
        ///     and generate tokens list from it.
        /// </summary>
        Lexing = 1,

        /// <summary>
        ///     Generate Abstract Syntax Tree
        ///     from tokens list.
        /// </summary>
        Parsing,

        /// <summary>
        ///     Reduce generated syntax tree.
        /// </summary>
        Reduction,

        /// <summary>
        ///     Convert Axion source into source code
        ///     in another language.
        /// </summary>
        Translation
    }
}
