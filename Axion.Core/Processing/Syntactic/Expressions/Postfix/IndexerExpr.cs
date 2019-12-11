using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         index_expr:
    ///             atom '[' (infix_expr | slice) {',' (infix_expr | slice)} [','] ']';
    ///         slice:
    ///             [infix_expr] ':' [infix_expr] [':' [infix_expr]];
    ///     </c>
    /// </summary>
    public class IndexerExpr : Expr {
        private Expr target;

        public Expr Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expr index;

        public Expr Index {
            get => index;
            set => SetNode(ref index, value);
        }

        public IndexerExpr(Expr parent, Expr target = null) : base(parent) {
            Target = target;
        }

        public IndexerExpr Parse() {
            if (Target == null) {
                Target = Parsing.ParseAtom(this);
            }

            SetSpan(() => {
                var expressions = new NodeList<Expr>(this);
                Stream.Eat(OpenBracket);
                if (!Stream.PeekIs(CloseBracket)) {
                    while (true) {
                        Expr start = null;
                        if (!Stream.PeekIs(Colon)) {
                            start = Parsing.ParseInfix(this);
                        }

                        if (Stream.MaybeEat(Colon)) {
                            Expr stop = null;
                            if (!Stream.PeekIs(Colon, Comma, CloseBracket)) {
                                stop = Parsing.ParseInfix(this);
                            }

                            Expr step = null;
                            if (Stream.MaybeEat(Colon)
                             && !Stream.PeekIs(Comma, CloseBracket)) {
                                step = Parsing.ParseInfix(this);
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

                Index = Parsing.MaybeTuple(expressions);
                Stream.Eat(CloseBracket);
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Target);
            if (Index is SliceExpr) {
                c.Write(Index);
            }
            else {
                c.Write("[", Index, "]");
            }
        }

        public override void ToCSharp(CodeWriter c) {
            ToAxion(c);
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }

        public override void ToPascal(CodeWriter c) {
            ToAxion(c);
        }
    }
}