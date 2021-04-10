using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <code>
    ///         while-expr:
    ///             'while' infix-expr scope
    ///             ['nobreak' scope];
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class WhileExpr : Node, IDecorableExpr {
        [LeafSyntaxNode] Token? kwWhile;
        [LeafSyntaxNode] Node condition = null!;
        [LeafSyntaxNode] ScopeExpr scope = null!;
        [LeafSyntaxNode] Token? kwNoBreak;
        [LeafSyntaxNode] ScopeExpr? noBreakScope;

        public WhileExpr(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Node[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Node>(this, items)
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
}
