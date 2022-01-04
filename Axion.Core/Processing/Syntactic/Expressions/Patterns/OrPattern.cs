using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns;

/// <summary>
///     <code>
///         or-pattern:
///             syntax-pattern '|' syntax-pattern;
///     </code>
/// </summary>
[Branch]
public partial class OrPattern : Pattern {
    [Leaf] Pattern? left;
    [Leaf] Pattern right = null!;

    public OrPattern(Node parent) : base(parent) { }

    public override bool Match(MacroMatchExpr match) {
        return (Left?.Match(match) ?? false) || Right.Match(match);
    }

    public OrPattern Parse() {
        Left ??= new CascadePattern(this).Parse();
        Stream.Eat(Pipe);
        Right = new CascadePattern(this).Parse();
        return this;
    }
}
