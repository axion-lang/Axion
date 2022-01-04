using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns;

/// <summary>
///     <code>
///         optional-pattern:
///             '[' syntax-pattern ']';
///     </code>
/// </summary>
[Branch]
public partial class OptionalPattern : Pattern {
    [Leaf] Pattern pattern = null!;

    public OptionalPattern(Node parent) : base(parent) { }

    public override bool Match(MacroMatchExpr match) {
        Pattern.Match(match);
        return true;
    }

    public OptionalPattern Parse() {
        Stream.Eat(OpenBracket);
        Pattern = new CascadePattern(this).Parse();
        Stream.Eat(CloseBracket);
        return this;
    }
}
