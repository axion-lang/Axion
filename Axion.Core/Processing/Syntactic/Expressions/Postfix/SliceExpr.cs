using Magnolia.Attributes;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix;

/// <summary>
///     <code>
///         slice-expr:
///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
///     </code>
/// </summary>
[Branch]
public partial class SliceExpr : Node {
    [Leaf] Node? from;
    [Leaf] Node? step;
    [Leaf] Node? to;

    public SliceExpr(Node parent) : base(parent) { }
}
