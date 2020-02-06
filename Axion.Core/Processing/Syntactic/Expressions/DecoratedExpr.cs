using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class DecoratedExpr : Expr, IDecoratedExpr {
        private NodeList<Expr> decorators;

        public NodeList<Expr> Decorators {
            get => decorators;
            set => SetNode(ref decorators, value);
        }

        private Expr target;

        public Expr Target {
            get => target;
            set => SetNode(ref target, value);
        }

        internal DecoratedExpr(
            Expr              parent     = null,
            IEnumerable<Expr> decorators = null,
            Expr              target     = null
        ) : base(parent) {
            Decorators = NodeList<Expr>.From(this, decorators);
            Target     = target;
        }

        public DecoratedExpr Parse() {
            SetSpan(() => {
                Stream.Eat(At);
                if (Stream.MaybeEat(OpenBracket)) {
                    do {
                        Decorators.Add(InfixExpr.Parse(this));
                    } while (Stream.MaybeEat(Comma));

                    Stream.Eat(CloseBracket);
                }
                else {
                    Decorators.Add(InfixExpr.Parse(this));
                }

                Target = AnyExpr.Parse(this);
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            if (Decorators.Count == 1) {
                c.Write("@", Decorators[0], Target);
            }
            else {
                c.Write("@[");
                c.AddJoin(", ", Decorators);
                c.WriteLine("]");
                c.Write(Target);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            foreach (Expr decorator in Decorators) {
                if (decorator is NameExpr n
                 && Spec.CSharp.AllowedModifiers.Contains(n.ToString())) {
                    c.Write(n, " ");
                }
            }

            c.Write(Target);
        }

        public override void ToPython(CodeWriter c) {
            foreach (Expr decorator in Decorators) {
                c.WriteLine("@", decorator);
            }

            c.Write(Target);
        }

        public override void ToPascal(CodeWriter c) { }
    }
}