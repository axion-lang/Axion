using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         index_expr:
    ///             atom '[' (preglobal_expr | slice) {',' (preglobal_expr | slice)} [','] ']'
    ///         slice:
    ///             [preglobal_expr] ':' [preglobal_expr] [':' [preglobal_expr]]
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

        public override TypeName ValueType => throw new NotImplementedException();

        public IndexerExpression(AstNode parent, Expression target) : base(parent) {
            MarkStart(Target = target);

            var expressions = new NodeList<Expression>(parent);
            parent.Eat(OpenBracket);
            if (!parent.Peek.Is(CloseBracket)) {
                while (true) {
                    Expression start = null;
                    if (!parent.Peek.Is(Colon)) {
                        start = ParseInfixExpr(parent);
                    }

                    if (parent.MaybeEat(Colon)) {
                        Expression stop = null;
                        if (!parent.Peek.Is(Colon, Comma, CloseBracket)) {
                            stop = ParseInfixExpr(parent);
                        }

                        Expression step = null;
                        if (parent.MaybeEat(Colon)
                            && !parent.Peek.Is(Comma, CloseBracket)) {
                            step = ParseInfixExpr(parent);
                        }

                        expressions.Add(new SliceExpression(parent, start, stop, step));
                        break;
                    }

                    if (start == null) {
                        parent.Unit.Blame(BlameType.InvalidIndexerExpression, parent.Token);
                    }

                    expressions.Add(start);
                    if (parent.Peek.Is(CloseBracket)) {
                        break;
                    }

                    Eat(Comma);
                }
            }

            Index = MaybeTuple(parent, expressions);
            MarkEndAndEat(CloseBracket);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Target, "[", Index, "]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Target, "[", Index, "]");
        }
    }
}