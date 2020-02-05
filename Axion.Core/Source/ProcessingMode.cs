namespace Axion.Core.Source {
    /// <summary>
    ///     Determines how compiler must
    ///     process source unit.
    /// </summary>
    public enum ProcessingMode {
        None,

        /// <summary>
        ///     Perform lexical analysis on source
        ///     and generate tokens list from it.
        /// </summary>
        Lexing,

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
        Transpilation
    }
}