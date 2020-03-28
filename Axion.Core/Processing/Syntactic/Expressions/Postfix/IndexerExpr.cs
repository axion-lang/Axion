using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         index-expr:
    ///             atom '[' (infix-expr | slice) {',' (infix-expr | slice)} [','] ']';
    ///         slice:
    ///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
    ///     </c>
    /// </summary>
    public class IndexerExpr : PostfixExpr {
        private Expr target;

        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        private Expr index;

        public Expr Index {
            get => index;
            set => index = Bind(value);
        }

        public IndexerExpr(
            Expr? parent = null,
            Expr? target = null
        ) : base(
            parent
         ?? GetParentFromChildren(target)
        ) {
            Target = target;
        }

        public IndexerExpr Parse() {
            if (Target == null) {
                Target = AtomExpr.Parse(this);
            }

            SetSpan(
                () => {
                    var expressions = new NodeList<Expr>(this);
                    Stream.Eat(OpenBracket);
                    if (!Stream.PeekIs(CloseBracket)) {
                        while (true) {
                            Expr start = null;
                            if (!Stream.PeekIs(Colon)) {
                                start = InfixExpr.Parse(this);
                            }

                            if (Stream.MaybeEat(Colon)) {
                                Expr stop = null;
                                if (!Stream.PeekIs(Colon, Comma, CloseBracket)) {
                                    stop = InfixExpr.Parse(this);
                                }

                                Expr step = null;
                                if (Stream.MaybeEat(Colon)
                                 && !Stream.PeekIs(Comma, CloseBracket)) {
                                    step = InfixExpr.Parse(this);
                                }

                                expressions.Add(new SliceExpr(this, start, stop, step));
                                break;
                            }

                            if (start == null) {
                                LangException.Report(BlameType.InvalidIndexerExpression, Stream.Token);
                            }

                            expressions.Add(start);
                            if (Stream.PeekIs(CloseBracket)) {
                                break;
                            }

                            Stream.Eat(Comma);
                        }
                    }
                    Index = expressions.Count == 1 ? expressions[0] : new TupleExpr(this, expressions);
                    Stream.Eat(CloseBracket);
                }
            );
            return this;
        }

        public override void ToDefault(CodeWriter c) {
            c.Write(Target);
            if (Index is SliceExpr) {
                c.Write(Index);
            }
            else {
                c.Write("[", Index, "]");
            }
        }
    }
}