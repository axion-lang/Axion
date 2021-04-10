using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <code>
    ///         unary-expr:
    ///             UNARY-LEFT prefix-expr
    ///             | suffix-expr UNARY-RIGHT;
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class UnaryExpr : PostfixExpr {
        [LeafSyntaxNode] Node value = null!;
        [LeafSyntaxNode] OperatorToken @operator = null!;

        public UnaryExpr(Node parent) : base(parent) { }
    }
}
