using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns;

/// <summary>
///     <code>
///         token-pattern:
///             STRING;
///     </code>
/// </summary>
[Branch]
public partial class TokenPattern : Pattern {
    internal Token? Value { get; set; }

    public TokenPattern(Node parent) : base(parent) { }

    public override bool Match(MacroMatchExpr match) {
        var s = match.Unit.TokenStream;
        if (s.Peek.Content != Value?.Content) {
            return false;
        }

        match.Nodes.Add(s.Eat());
        return true;
    }

    public TokenPattern Parse() {
        Value = Stream.Eat();
        Unit.Module?.RegisterCustomKeyword(Value.Content);
        return this;
    }
}
