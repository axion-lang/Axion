using Axion.SourceGenerators;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <code>
    ///         slice-expr:
    ///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class SliceExpr : Node {
        [LeafSyntaxNode] Node? from;
        [LeafSyntaxNode] Node? to;
        [LeafSyntaxNode] Node? step;

        public SliceExpr(Node parent) : base(parent) { }
    }
}
