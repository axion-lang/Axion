using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <code>
    ///         code-quote-expr:
    ///             '{{' any '}}';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class CodeQuoteExpr : AtomExpr {
        [LeafSyntaxNode] Token? openQuote;
        [LeafSyntaxNode] ScopeExpr scope = null!;
        [LeafSyntaxNode] Token? closeQuote;

        public override TypeName? ValueType => Scope.ValueType;

        public CodeQuoteExpr(Node parent) : base(parent) { }

        public CodeQuoteExpr Parse() {
            OpenQuote = Stream.Eat(DoubleOpenBrace);
            Scope     = new ScopeExpr(this);
            while (!Stream.PeekIs(DoubleCloseBrace, TokenType.End)) {
                Scope.Items += AnyExpr.Parse(this);
            }
            CloseQuote = Stream.Eat(DoubleCloseBrace);
            return this;
        }
    }
}
