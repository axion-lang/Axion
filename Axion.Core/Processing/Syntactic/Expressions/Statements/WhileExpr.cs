using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

/// <summary>
///     <code>
///         while-expr:
///             'while' infix-expr scope
///             ['nobreak' scope];
///     </code>
/// </summary>
[Branch]
public partial class WhileExpr : Node, IDecorableExpr {
    [Leaf] Node condition = null!;
    [Leaf] Token? kwNoBreak;
    [Leaf] Token? kwWhile;
    [Leaf] ScopeExpr? noBreakScope;
    [Leaf] ScopeExpr scope = null!;

    public WhileExpr(Node parent) : base(parent) { }

    public DecoratedExpr WithDecorators(params Node[] items) {
        return new(Parent) {
            Target     = this,
            Decorators = new NodeList<Node, Ast>(this, items)
        };
    }

    public Node Parse() {
        KwWhile   = Stream.Eat(KeywordWhile);
        Condition = InfixExpr.Parse(this);
        Scope     = new ScopeExpr(this).Parse();
        if (Stream.MaybeEat("no-break")) {
            KwNoBreak    = Stream.Token;
            NoBreakScope = new ScopeExpr(this).Parse();
        }

        return this;
    }
}
