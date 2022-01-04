using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions;

[Branch]
public partial class DecoratedExpr : Node {
    [Leaf] NodeList<Node, Ast>? decorators;
    [Leaf] Token? startMark;
    [Leaf] Node? target;

    internal DecoratedExpr(Node? parent) : base(parent) { }

    public DecoratedExpr Parse() {
        StartMark = Stream.Eat(At);
        if (Stream.MaybeEat(OpenBracket)) {
            do {
                Decorators += InfixExpr.Parse(this);
            } while (Stream.MaybeEat(Comma));

            Stream.Eat(CloseBracket);
        }
        else {
            Decorators += PrefixExpr.Parse(this);
        }

        Target = AnyExpr.Parse(this);
        return this;
    }
}
