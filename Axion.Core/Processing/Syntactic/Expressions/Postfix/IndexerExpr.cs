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
        private Expr? index;

        public Expr? Index {
            get => index;
            set => index = BindNullable(value);
        }

        private Expr? target;

        public Expr? Target {
            get => target;
            set => target = BindNullable(value);
        }

        public IndexerExpr(Node parent) : base(parent) { }

        public IndexerExpr Parse() {
            Target ??= AtomExpr.Parse(this);
            var expressions = new NodeList<Expr>(this);
            Stream.Eat(OpenBracket);
            if (!Stream.PeekIs(CloseBracket)) {
                while (true) {
                    Expr? start = null;
                    if (!Stream.PeekIs(Colon)) {
                        start = InfixExpr.Parse(this);
                    }

                    if (Stream.MaybeEat(Colon)) {
                        Expr? stop = null;
                        if (!Stream.PeekIs(
                            Colon,
                            Comma,
                            CloseBracket
                        )) {
                            stop = InfixExpr.Parse(this);
                        }

                        Expr? step = null;
                        if (Stream.MaybeEat(Colon)
                         && !Stream.PeekIs(Comma, CloseBracket)) {
                            step = InfixExpr.Parse(this);
                        }

                        expressions += new SliceExpr(this) {
                            From = start,
                            To   = stop,
                            Step = step
                        };
                        break;
                    }

                    if (start == null) {
                        LanguageReport.To(
                            BlameType.InvalidIndexerExpression,
                            Stream.Token
                        );
                    }
                    else {
                        expressions += start;
                    }

                    if (Stream.PeekIs(CloseBracket)) {
                        break;
                    }

                    Stream.Eat(Comma);
                }
            }

            Index = expressions.Count == 1
                ? expressions[0]
                : new TupleExpr(this) {
                    Expressions = expressions
                };
            End = Stream.Eat(CloseBracket).End;
            return this;
        }
    }
}
