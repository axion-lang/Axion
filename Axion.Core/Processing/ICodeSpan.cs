using Axion.Core.Hierarchy;

namespace Axion.Core.Processing;

public interface ICodeSpan {
    public Unit Unit { get; set; }

    /// <summary>
    ///     Start location of this node's code span.
    /// </summary>
    public Location Start { get; set; }

    /// <summary>
    ///     End location of this node's code span.
    /// </summary>
    public Location End { get; set; }
}
