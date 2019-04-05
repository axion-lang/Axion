using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parenthesis_expr ::=
    ///             '(' ')'
    ///             | yield_expr
    ///             | test_list
    ///             | generator_expr
    ///     </c>
    /// </summary>
    public class ParenthesisExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal ParenthesisExpression(Expression expression) {
            Value = expression;
            MarkPosition(expression);
        }

        internal static Expression Parse(SyntaxTreeNode parent) {
            Token start = parent.StartNode(TokenType.OpenParenthesis);

            Expression result;
            // empty tuple
            if (parent.PeekIs(TokenType.CloseParenthesis)) {
                result = new TupleExpression(false, null, start, parent.Token);
            }
            else {
                if (parent.PeekIs(TokenType.KeywordYield)) {
                    // yield expression
                    result = new YieldExpression(parent);
                }
                else {
                    Expression expr = ParseExpression(parent);
                    // tuple
                    if (parent.MaybeEat(TokenType.Comma)) {
                        var list = new TestList(parent, out bool trailingComma);
                        list.Insert(0, expr);
                        result = new TupleExpression(!trailingComma, list.Expressions);
                    }
                    // generator
                    else if (parent.PeekIs(TokenType.KeywordFor)) {
                        result = new GeneratorExpression(parent, expr);
                    }
                    // parenthesized
                    else {
                        result = expr is ParenthesisExpression
                                 || expr is TupleExpression
                            ? expr
                            : new ParenthesisExpression(expr);
                    }
                }
            }

            parent.Eat(TokenType.CloseParenthesis);
            result.MarkEnd(parent.Token);
            return result;
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "(" + Value + ")";
        }
    }
}