using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         list_expr ::=
    ///             '[' comprehension | (expr {',' expr} [',']) ']'
    ///     </c>
    /// </summary>
    public class ListExpression : MultipleExpression<Expression> {
        public ListExpression(NodeList<Expression> expressions) {
            Expressions = expressions ?? new NodeList<Expression>(this);
        }

        internal ListExpression(NodeList<Expression> expressions, Token start, Token end)
            : this(expressions) {
            MarkPosition(start, end);
        }

        internal ListExpression(SyntaxTreeNode parent) {
            Parent      = parent;
            Expressions = new NodeList<Expression>(this);

            StartNode(TokenType.OpenBracket);

            if (!MaybeEat(TokenType.CloseBracket)) {
                Expressions.Add(ParseTestExpr(this));
                if (MaybeEat(TokenType.Comma)) {
                    Expressions.AddRange(new TestList(this, out bool _).Expressions);
                }
                else if (PeekIs(TokenType.KeywordFor)) {
                    Expressions[0] = new ForComprehension(this);
                }
            }

            Eat(TokenType.CloseBracket);
            MarkEnd(Token);
        }
    }
}