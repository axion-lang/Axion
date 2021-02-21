using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class DecoratedExpr : Expr {
        private Token? startMark;

        public Token? StartMark {
            get => startMark;
            set => startMark = BindNullable(value);
        }

        private NodeList<Expr>? decorators;

        public NodeList<Expr> Decorators {
            get => InitIfNull(ref decorators);
            set => decorators = Bind(value);
        }

        private Expr? target;

        public Expr? Target {
            get => target;
            set => target = BindNullable(value);
        }

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
