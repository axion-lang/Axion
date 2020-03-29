using System.Collections.Generic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class DecorableExpr : Expr, IDecorableExpr {
        private NodeList<Expr> decorators;

        public NodeList<Expr> Decorators {
            get => decorators;
            set => decorators = Bind(value);
        }

        private Expr target;

        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        internal DecorableExpr(
            Expr?              parent     = null,
            IEnumerable<Expr>? decorators = null,
            Expr?              target     = null
        ) : base(
            parent
         ?? GetParentFromChildren(target)
        ) {
            Decorators = NodeList<Expr>.From(this, decorators);
            Target     = target;
        }

        public DecorableExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(At);
                    if (Stream.MaybeEat(OpenBracket)) {
                        do {
                            Decorators.Add(InfixExpr.Parse(this));
                        } while (Stream.MaybeEat(Comma));

                        Stream.Eat(CloseBracket);
                    }
                    else {
                        Decorators.Add(PrefixExpr.Parse(this));
                    }

                    Target = AnyExpr.Parse(this);
                }
            );
            return this;
        }
    }
}