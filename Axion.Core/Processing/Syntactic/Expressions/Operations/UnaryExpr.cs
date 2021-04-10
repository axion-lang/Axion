using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         unary-expr:
    ///             UNARY-LEFT prefix-expr
    ///             | suffix-expr UNARY-RIGHT;
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class UnaryExpr : PostfixExpr {
        [LeafSyntaxNode] Node value = null!;
        [LeafSyntaxNode] OperatorToken @operator = null!;

        public UnaryExpr(Node parent) : base(parent) { }
    }
}
