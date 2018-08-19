using System;

namespace Axion.Processing {
    /// <summary>
    ///     Defines Options of processing of source code.
    ///     USE '&amp;' AND '&amp;=' OPERATORS TO DEFINE AND APPEND FLAGS!
    /// </summary>
    [Flags]
    public enum SourceProcessingOptions {
        None = 0,
        // INDENTATION
        CheckIndentationConsistency = 0b100000000,
        TabSize1                    = 0b100000001,
        TabSize2                    = 0b100000010,
        TabSize3                    = 0b100000100,
        TabSize4                    = 0b100001000,
        TabSize5                    = 0b100010000,
        TabSize6                    = 0b100100000,
        TabSize7                    = 0b101000000,
        TabSize8                    = 0b110000000,
    }
}