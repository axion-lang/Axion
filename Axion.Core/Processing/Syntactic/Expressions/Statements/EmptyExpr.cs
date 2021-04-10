using Axion.Core.Processing.Lexical.Tokens;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         empty-expr:
    ///             ';' | 'pass';
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class EmptyExpr : Node {
        [LeafSyntaxNode] Token? mark;

        public EmptyExpr(Node parent) : base(parent) { }

        public EmptyExpr Parse() {
            Mark = Stream.Eat(Semicolon, KeywordPass);
            return this;
        }
    }
}
