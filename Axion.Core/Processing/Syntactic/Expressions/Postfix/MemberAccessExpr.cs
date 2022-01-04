using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix;

/// <summary>
///     <code>
///         member-expr:
///             atom '.' ID;
///     </code>
/// </summary>
[Branch]
public partial class MemberAccessExpr : PostfixExpr {
    [Leaf] Node? member;
    [Leaf] Node? target;

    public MemberAccessExpr(Node parent) : base(parent) { }

    public MemberAccessExpr Parse() {
        Target ??= AtomExpr.Parse(this);
        Stream.Eat(Dot);
        Member = AtomExpr.Parse(this);
        return this;
    }

    // TODO: check for accessing prop/field existence
}
