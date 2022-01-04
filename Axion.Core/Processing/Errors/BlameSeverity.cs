namespace Axion.Core.Processing.Errors;

public enum BlameSeverity {
    /// <summary>
    ///     Errors that are fixed by compiler automatically,
    ///     but reducing the source code quality.
    /// </summary>
    Info,

    /// <summary>
    ///     Non-critical errors
    ///     (e.g. low-performance solutions, redundancies, etc.),
    ///     that cannot be fixed by compiler automatically.
    /// </summary>
    Warning,

    /// <summary>
    ///     Errors that prevent the source code from successful compiling.
    /// </summary>
    Error
}
