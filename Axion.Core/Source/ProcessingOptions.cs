using System;

namespace Axion.Core.Source {
    /// <summary>
    ///     Defines some settings for source code processing.
    /// </summary>
    [Flags]
    public enum ProcessingOptions {
        /// <summary>
        ///     No additional options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Source is processed by default.
        /// </summary>
        Default = CheckIndentationConsistency,

        /// <summary>
        ///     Check code against inconsistent
        ///     indentation (mixed spaced and tabs).
        /// </summary>
        CheckIndentationConsistency = 0b1,

        /// <summary>
        ///     Conversion Axion source back into
        ///     Axion source (debugging mode).
        /// </summary>
        ToAxion = 0b01000,

        /// <summary>
        ///     Conversion Axion source into
        ///     C# programming language source.
        /// </summary>
        ToCSharp = 0b00100,

        /// <summary>
        ///     Conversion Axion source into
        ///     Python programming language source.
        /// </summary>
        ToPython = 0b00010,

        /// <summary>
        ///     Conversion Axion source into
        ///     Pascal programming language source.
        /// </summary>
        ToPascal = 0b00001
    }
}