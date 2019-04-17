namespace Axion.Core.Processing {
    /// <summary>
    ///     Determines how compiler must
    ///     process source unit.
    /// </summary>
    public enum SourceProcessingMode {
        None = 0,

        /// <summary>
        ///     Perform lexical analysis on source
        ///     and generate tokens list from it.
        /// </summary>
        Lex = 0b000000001,

        /// <summary>
        ///     Perform lexical analysis on source,
        ///     then generate AST (Abstract Syntax Tree)
        ///     from tokens list.
        /// </summary>
        Parsing = 0b000000011,

        /// <summary>
        ///     Do interactive processing on source
        ///     and output result.
        /// </summary>
        Interpret = 0b000000111,

        /// <summary>
        ///     Compile Axion source into machine code.
        /// </summary>
        Compile = 0b000001011,

        /// <summary>
        ///     Conversion Axion source into
        ///     C# programming language source.
        /// </summary>
        ConvertCS = 0b001000011,

        /// <summary>
        ///     Conversion Axion source into
        ///     C programming language source.
        /// </summary>
        ConvertC = 0b000010011,

        /// <summary>
        ///     Conversion Axion source into
        ///     C++ programming language source.
        /// </summary>
        ConvertCpp = 0b000100011,

        /// <summary>
        ///     Conversion Axion source into
        ///     JavaScript programming language source.
        /// </summary>
        ConvertJS = 0b010000011,

        /// <summary>
        ///     Conversion Axion source into
        ///     Python programming language source.
        /// </summary>
        ConvertPy = 0b100000011
    }
}