using System;

namespace Axion.Core.Source {
    /// <summary>
    ///     Defines some settings for source code processing.
    /// </summary>
    [Flags]
    public enum ProcessingOptions {
        /// <summary>
        ///     Source is processed by default.
        /// </summary>
        None = 0,

        Default = CheckIndentationConsistency,

        /// <summary>
        ///     Check code against inconsistent
        ///     indentation (mixed spaced and tabs).
        /// </summary>
        CheckIndentationConsistency = 1 << 2,

        /// <summary>
        ///     Conversion Axion source back into
        ///     Axion source (debugging mode).
        /// </summary>
        ToAxion,

        /// <summary>
        ///     Conversion Axion source into
        ///     C# programming language source.
        /// </summary>
        ToCSharp,

        /// <summary>
        ///     Conversion Axion source into
        ///     Python programming language source.
        /// </summary>
        ToPython,

        /// <summary>
        ///     Conversion Axion source into
        ///     Pascal programming language source.
        /// </summary>
        ToPascal
    }
}