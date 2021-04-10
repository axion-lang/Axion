using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    [SyntaxExpression]
    public partial class DecoratedExpr : Node {
        [LeafSyntaxNode] Token? startMark;
        [LeafSyntaxNode] NodeList<Node>? decorators;
        [LeafSyntaxNode] Node? target;

        internal DecoratedExpr(Node parent) : base(parent) { }

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
}
