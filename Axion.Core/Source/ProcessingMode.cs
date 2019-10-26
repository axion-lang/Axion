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
        Lex,

        /// <summary>
        ///     Perform lexical analysis on source,
        ///     then generate AST (Abstract Syntax Tree)
        ///     from tokens list.
        /// </summary>
        Parsing,
        
        /// <summary>
        ///     Traverse (reduce) previously generate
        ///     syntax tree.
        /// </summary>
        Traversing,

        /// <summary>
        ///     Do interactive processing on source
        ///     and output result.
        /// </summary>
        Interpret,

        /// <summary>
        ///     Compile Axion source into machine code.
        /// </summary>
        Compile,

        /// <summary>
        ///     Conversion Axion source back into
        ///     Axion source (debugging mode).
        /// </summary>
        ConvertAxion,

        /// <summary>
        ///     Conversion Axion source into
        ///     C# programming language source.
        /// </summary>
        ConvertCS,

        /// <summary>
        ///     Conversion Axion source into
        ///     C++ programming language source.
        /// </summary>
        ConvertCpp,

        /// <summary>
        ///     Conversion Axion source into
        ///     JavaScript programming language source.
        /// </summary>
        ConvertJS,

        /// <summary>
        ///     Conversion Axion source into
        ///     Python programming language source.
        /// </summary>
        ConvertPy,

        /// <summary>
        ///     Conversion Axion source into
        ///     Pascal programming language source.
        /// </summary>
        ConvertPas
    }
}