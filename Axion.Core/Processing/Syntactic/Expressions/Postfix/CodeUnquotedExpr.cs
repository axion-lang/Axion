using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using Axion.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         code-unquoted-expr:
    ///             '$' expr;
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class CodeUnquotedExpr : PostfixExpr {
        [LeafSyntaxNode] Token? startMark;
        [LeafSyntaxNode] Node value = null!;

        public override TypeName? ValueType => Value.ValueType;

        public CodeUnquotedExpr(Node parent) : base(parent) { }

        public CodeUnquotedExpr Parse() {
            StartMark = Stream.Eat(TokenType.Dollar);
            Value     = Parse(this);
            return this;
        }
    }
}
