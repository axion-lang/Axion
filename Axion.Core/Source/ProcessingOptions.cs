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

        /// <summary>
        ///     Check code against inconsistent
        ///     indentation (mixed spaced and tabs).
        /// </summary>
        CheckIndentationConsistency = 1,

        /// <summary>
        ///     Save debugging information to files
        ///     when performing code syntax analysis.
        /// </summary>
        SyntaxAnalysisDebugOutput = 1 << 1
    }
}