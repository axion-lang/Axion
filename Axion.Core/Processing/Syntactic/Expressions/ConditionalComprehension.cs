using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parent  ('if' | 'unless') operation
    ///     </c>
    ///     Closes comprehensions tree,
    ///     cannot be continued with other comprehensions.
    /// </summary>
    public class ConditionalComprehension : Expression {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        internal ConditionalComprehension(SyntaxTreeNode left) {
            Parent = left;
            if (PeekIs(TokenType.KeywordIf)) {
                StartNode(TokenType.KeywordIf);
                Condition = ParseOperation(this);
            }
            else {
                StartNode(TokenType.KeywordUnless);
                Condition = new UnaryExpression(TokenType.OpNot, ParseOperation(this));
            }

            MarkEnd(Token);
        }
    }
}