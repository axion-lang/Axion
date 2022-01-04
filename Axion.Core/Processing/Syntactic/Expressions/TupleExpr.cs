using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions;

/// <summary>
///     <code>
///         tuple-expr:
///             tuple-paren-expr | (multiple-expr [',']);
///         tuple-paren-expr:
///             '(' multiple-expr [','] ')';
///     </code>
/// </summary>
[Branch]
public partial class TupleExpr : AtomExpr {
    [Leaf] NodeList<Node, Ast>? expressions;

    public override TypeName InferredType =>
        new TupleTypeName(this) {
            Types = new NodeList<TypeName, Ast>(
                this,
                Expressions.Where(e => e.InferredType != null)
                    .Select(e => e.InferredType!)
            )
        };

    internal TupleExpr(Node parent) : base(parent) { }

    public TupleExpr ParseEmpty() {
        Stream.Eat(OpenParenthesis);
        Stream.Eat(CloseParenthesis);
        return this;
    }
}
