using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic;

/// <summary>
///     <code>
///         code-quote-expr:
///             '{{' any '}}';
///     </code>
/// </summary>
[Branch]
public partial class CodeQuoteExpr : AtomExpr {
    [Leaf] Token? closeQuote;
    [Leaf] Token? openQuote;
    [Leaf] ScopeExpr scope = null!;

    public override TypeName? InferredType => Scope.InferredType;

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
