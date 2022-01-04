using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns;

/// <summary>
///     <code>
///         group-pattern:
///             '(' syntax-pattern ')';
///     </code>
/// </summary>
[Branch]
public partial class GroupPattern : Pattern {
    [Leaf] Pattern pattern = null!;

    public GroupPattern(Node parent) : base(parent) { }

    public override bool Match(MacroMatchExpr match) {
        return Pattern.Match(match);
    }

    public GroupPattern Parse() {
        Stream.Eat(OpenParenthesis);
        Pattern = new CascadePattern(this).Parse();
        Stream.Eat(CloseParenthesis);
        return this;
    }
}
