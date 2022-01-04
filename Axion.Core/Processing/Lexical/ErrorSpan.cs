using Axion.Core.Hierarchy;

namespace Axion.Core.Processing.Lexical;

public class ErrorSpan : ICodeSpan {
    public Unit Unit { get; set; } = null!;
    public Location Start { get; set; }
    public Location End { get; set; }
}
