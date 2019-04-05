using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class IndexExpression : Expression {
        private Expression target;

        [NotNull]
        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expression index;

        [NotNull]
        public Expression Index {
            get => index;
            set => SetNode(ref index, value);
        }

        public IndexExpression([NotNull] Expression target, [NotNull] Expression index) {
            Target = target;
            Index  = index;

            MarkPosition(target, index);
        }

        /// <summary>
        ///     <c>
        ///         indexer ::=
        ///             '[' (expr | slice) {',' (expr | slice)} [','] ']'
        ///         slice ::=
        ///             [expr] ':' [expr] [':' [expr]]
        ///     </c>
        /// </summary>
        internal static Expression Parse(SyntaxTreeNode parent) {
            parent.Eat(TokenType.OpenBracket);

            bool trailingComma;
            var  expressions = new NodeList<Expression>(parent);
            do {
                Expression start = null;
                if (!parent.PeekIs(TokenType.Colon)) {
                    start = ParseTestExpr(parent);
                }

                if (parent.MaybeEat(TokenType.Colon)) {
                    Expression stop = null;
                    if (!parent.PeekIs(TokenType.Colon, TokenType.Comma, TokenType.CloseBracket)) {
                        stop = ParseTestExpr(parent);
                    }

                    Expression step = null;
                    if (parent.MaybeEat(TokenType.Colon)
                        && !parent.PeekIs(TokenType.Comma, TokenType.CloseBracket)) {
                        step = ParseTestExpr(parent);
                    }

                    return new SliceExpression(start, stop, step);
                }

                expressions.Add(start);
                trailingComma = parent.MaybeEat(TokenType.Comma);
            } while (trailingComma && !parent.PeekIs(TokenType.CloseBracket));

            parent.Eat(TokenType.CloseBracket);
            return MaybeTuple(expressions, trailingComma);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Target + "[" + Index + "]";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + Target + "[" + Index + "]";
        }
    }
}