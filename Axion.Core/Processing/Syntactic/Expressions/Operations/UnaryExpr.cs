using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations;

/// <summary>
///     <code>
///         unary-expr:
///             UNARY-LEFT prefix-expr
///             | suffix-expr UNARY-RIGHT;
///     </code>
/// </summary>
[Branch]
public partial class UnaryExpr : PostfixExpr {
    [Leaf] OperatorToken @operator = null!;
    [Leaf] Node value = null!;

    public UnaryExpr(Node? parent) : base(parent) { }
}
