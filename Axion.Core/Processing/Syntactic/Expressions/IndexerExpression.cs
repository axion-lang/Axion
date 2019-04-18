using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         index_expr:
    ///             primary '[' (expr | slice) {',' (expr | slice)} [','] ']'
    ///         slice:
    ///             [expr] ':' [expr] [':' [expr]]
    ///     </c>
    /// </summary>
    public class IndexerExpression : Expression {
        private Expression target;

        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expression index;

        public Expression Index {
            get => index;
            set => SetNode(ref index, value);
        }

        public IndexerExpression(SyntaxTreeNode parent, Expression target) : base(parent) {
            Target = target;
            Index  = index;

            parent.Eat(TokenType.OpenBracket);

            var expressions = new NodeList<Expression>(parent);
            do {
                Expression? start = null;
                if (!parent.PeekIs(TokenType.Colon)) {
                    start = ParseTestExpr(parent);
                }

                if (parent.MaybeEat(TokenType.Colon)) {
                    Expression? stop = null;
                    if (!parent.PeekIs(TokenType.Colon, TokenType.Comma, TokenType.CloseBracket)) {
                        stop = ParseTestExpr(parent);
                    }

                    Expression? step = null;
                    if (parent.MaybeEat(TokenType.Colon)
                        && !parent.PeekIs(TokenType.Comma, TokenType.CloseBracket)) {
                        step = ParseTestExpr(parent);
                    }

                    expressions.Add(new SliceExpression(parent, start, stop, step));
                    break;
                }

                if (start == null) {
                    parent.Unit.ReportError("Index expression expected.", parent.Token);
                }

                expressions.Add(start);
            } while (parent.MaybeEat(TokenType.Comma));

            parent.Eat(TokenType.CloseBracket);
            Index = MaybeTuple(parent, expressions);

            MarkPosition(Target, Index);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Target, "[", Index, "]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Target, "[", Index, "]");
        }
    }
}