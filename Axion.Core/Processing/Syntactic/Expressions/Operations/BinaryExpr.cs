using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <code>
    ///         binary-expr:
    ///             infix OPERATOR infix;
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class BinaryExpr : InfixExpr {
        [LeafSyntaxNode] Node? left;
        [LeafSyntaxNode] Node? right;
        [LeafSyntaxNode] Token? @operator;

        public BinaryExpr(Node parent) : base(parent) { }
    }
}
