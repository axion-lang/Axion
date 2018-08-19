namespace Axion.Processing {
    /// <summary>
    ///     Determines that compiler need to do with
    ///     <see cref="SourceCode" /> (e.g. <see cref="Compile" /> or <see cref="Interpret" />)
    ///     USE '&amp;' AND '&amp;=' OPERATORS TO DEFINE AND APPEND FLAGS!
    /// </summary>
    public enum SourceProcessingMode {
        /// <summary>
        ///     Perform lexical analysis on source and generate tokens list from it.
        /// </summary>
        Lex = 0b000000001,

        /// <summary>
        ///     Perform lexical analysis on source, then generate AST (Abstract Syntax Tree) from tokens list.
        /// </summary>
        Parsing = 0b000000011,

        /// <summary>
        ///     Interactive processing of source and outputting result.
        /// </summary>
        Interpret = 0b000000111,

        #region Yet unsupported modes

        /// <summary>
        ///     Compile Axion source into machine code.
        /// </summary>
        Compile = 0b000001011,

        /// <summary>
        ///     Transpile Axion source into the C programming language source.
        /// </summary>
        ConvertC = 0b000010011,

        /// <summary>
        ///     Transpile Axion source into the C++ programming language source.
        /// </summary>
        ConvertCpp = 0b000100011,

        /// <summary>
        ///     Transpile Axion source into the C# programming language source.
        /// </summary>
        ConvertCSharp = 0b001000011,

        /// <summary>
        ///     Transpile Axion source into the JavaScript programming language source.
        /// </summary>
        ConvertJavaScript = 0b010000011,

        /// <summary>
        ///     Transpile Axion source into the Python programming language source.
        /// </summary>
        ConvertPython = 0b100000011

        #endregion
    }
}