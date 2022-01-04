using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations;

/// <summary>
///     <code>
///         binary-expr:
///             infix OPERATOR infix;
///     </code>
/// </summary>
[Branch]
public partial class BinaryExpr : InfixExpr {
    [Leaf] Node? left;
    [Leaf] Token? @operator;
    [Leaf] Node? right;

    public BinaryExpr(Node parent) : base(parent) { }
}
