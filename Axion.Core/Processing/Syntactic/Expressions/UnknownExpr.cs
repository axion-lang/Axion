using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Specification;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions;

/// <summary>
///     <code>
///         unknown-expr:
///             TOKEN* (NEWLINE | END);
///     </code>
/// </summary>
[Branch]
public partial class UnknownExpr : AtomExpr {
    [Leaf] NodeList<Token, Ast>? tokens;

    public UnknownExpr(Node parent) : base(parent) { }

    public UnknownExpr Parse() {
        while (!Stream.PeekIs(Newline, TokenType.End)) {
            Tokens += Stream.Eat();
        }
        LanguageReport.To(BlameType.InvalidSyntax, this);
        return this;
    }
}
