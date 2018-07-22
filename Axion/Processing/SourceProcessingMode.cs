namespace Axion.Processing {
    /// <summary>
    ///     Determines that compiler need to do with
    ///     <see cref="SourceCode" /> (e.g. <see cref="Compile" /> or <see cref="Interpret" />)
    /// </summary>
    public enum SourceProcessingMode {
        /// <summary>
        ///     Perform lexical analysis on source and generate tokens list from it.
        /// </summary>
        Lex = 0x01,

        /// <summary>
        ///     Perform lexical analysis on source, then generate AST (Abstract Syntax Tree) from tokens list.
        /// </summary>
        Parsing = 0x02,

        /// <summary>
        ///     Interactive processing of source and outputting result.
        /// </summary>
        Interpret = 0x03,

        #region Yet unsupported modes

        /// <summary>
        ///     Compile Axion source into machine code.
        /// </summary>
        Compile = 0x04,

        /// <summary>
        ///     Transpile Axion source into the C programming language source.
        /// </summary>
        ConvertC = 0x05,

        /// <summary>
        ///     Transpile Axion source into the C++ programming language source.
        /// </summary>
        ConvertCpp = 0x06,

        /// <summary>
        ///     Transpile Axion source into the C# programming language source.
        /// </summary>
        ConvertCSharp = 0x07,

        /// <summary>
        ///     Transpile Axion source into the JavaScript programming language source.
        /// </summary>
        ConvertJavaScript = 0x08,

        /// <summary>
        ///     Transpile Axion source into the Python programming language source.
        /// </summary>
        ConvertPython = 0x09

        #endregion
    }
}