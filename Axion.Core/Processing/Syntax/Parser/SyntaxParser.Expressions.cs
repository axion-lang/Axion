using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        /// <c>
        ///     expr_stmt:
        ///         test_list_star_expr (AUG_ASSIGN (yield_expr|test_list) |
        ///         ('=' (yield_expr|test_list_star_expr))*)
        ///     test_list_star_expr:
        ///         (test|star_expr) (',' (test|star_expr))* [',']
        /// </c>
        /// </summary>
        private Statement ParseExpressionStmt() {
            Expression expr = ParseTestListStarExpr();
            if (expr is ErrorExpression) {
                stream.NextToken();
            }
            // TODO: insert var_def somewhere here...
            if (stream.PeekIs(TokenType.Assign)) {
                expr = FinishAssignments(expr);
            }
            if (stream.MaybeEat(Spec.AugmentedAssignOperators)) {
                var        op = (OperatorToken) stream.Token;
                Expression right;
                if (stream.MaybeEat(TokenType.KeywordYield)) {
                    right = ParseYield();
                }
                else {
                    right = ParseTestListAsExpr();
                }
                if (expr.CannotAugAssignReason != null) {
                    ReportError(expr.CannotAugAssignReason, expr);
                }
                return new AugmentedAssignStatement(expr, op, right);
            }
            return new ExpressionStatement(expr);
        }

        private Expression FinishAssignments(Expression right) {
            var left = new List<Expression>();

            while (stream.MaybeEat(TokenType.Assign)) {
                if (right.CannotAssignReason != null) {
                    ReportError(right.CannotAssignReason, right);
                }
                left.Add(right);

                if (stream.MaybeEat(TokenType.KeywordYield)) {
                    right = ParseYield();
                }
                else {
                    right = ParseTestListStarExpr();
                }
            }

            Debug.Assert(left.Count > 0);
            return new AssignmentExpression(left.ToArray(), right);
        }

        /// <summary>
        ///     yield_expr:
        ///         'yield' [yield_arg]
        ///     yield_arg:
        ///         'from' test | test_list
        ///     <para />
        ///     Peek if the next token is a 'yield' and parse a yield expression.
        ///     Else return null.
        ///     Called w/ yield already stream.Eaten.
        /// </summary>
        private Expression ParseYield() {
            // Mark that this function is actually a generator.
            // If we're in a generator expression, then we don't have a function yet.
            // g=((yield i) for i in range(5))
            // In that case, the genexp will mark IsGenerator. 
            FunctionDefinition current = ast.CurrentFunction;
            if (current != null) {
                current.IsGenerator = true;
            }

            Position start = tokenStart;

            // Parse expression list after yield. This can be:
            // 1) empty, in which case it becomes 'yield None'
            // 2) a single expression
            // 3) multiple expression, in which case it's wrapped in a tuple.
            Expression yieldExpr;

            var isYieldFrom = false;
            if (stream.MaybeEat(TokenType.KeywordFrom)) {
                yieldExpr = ParseTestExpr();
                yieldExpr.MarkPosition(start, tokenEnd);
                isYieldFrom = true;
            }
            else {
                List<Expression> l = ParseExpressionList(out bool trailingComma);
                if (l.Count == 0) {
                    // Check empty expression and convert to 'none'
                    // location set to match yield location
                    yieldExpr = new ConstantExpression(null, start, tokenEnd);
                }
                else if (l.Count != 1) {
                    // make a tuple
                    yieldExpr = MakeTupleOrExpr(l, trailingComma);
                }
                else {
                    // just take the single expression
                    yieldExpr = l[0];
                }
            }
            return new YieldExpression(yieldExpr, isYieldFrom, start, tokenEnd);
        }

        /// <summary>
        /// <c>
        ///     test:
        ///         or_test ['if' or_test 'else' test] |
        ///         lambda_def
        /// </c>
        /// </summary>
        private Expression ParseTestExpr(bool noInnerGeneratorOrComprehension = false) {
            Expression expr = ParseOrTest();
            if (!noInnerGeneratorOrComprehension
             && stream.PeekIs(TokenType.KeywordIf)
             && stream.Token.Type != TokenType.Newline) {
                expr = ParseConditionalTest(expr);
            }
            return expr;
        }

        #region Arithmetic precedence-based expressions

        /// <summary>
        /// <c>
        ///     'if' or_test ['else' test]
        /// </c>
        /// </summary>
        private Expression ParseConditionalTest(Expression trueExpr) {
            stream.Eat(TokenType.KeywordIf);
            Expression expr      = ParseOrTest();
            Expression falseExpr = null;
            if (stream.MaybeEat(TokenType.KeywordElse)) {
                falseExpr = ParseTestExpr();
            }
            return new ConditionalExpression(expr, trueExpr, falseExpr);
        }

        /// <summary>
        /// <c>
        ///     or_test:
        ///         and_test ('or' and_test)*
        /// </c>
        /// </summary>
        private Expression ParseOrTest() {
            Expression expr = ParseAndTest();
            while (stream.MaybeEat(TokenType.KeywordOr)) {
                expr = new BinaryExpression(expr, (OperatorToken) stream.Token, ParseAndTest());
            }
            return expr;
        }

        /// <summary>
        /// <c>
        ///     and_test:
        ///         not_test ('and' not_test)*
        /// </c>
        /// </summary>
        private Expression ParseAndTest() {
            Expression expr = ParseNotTest();
            while (stream.MaybeEat(TokenType.KeywordAnd)) {
                expr = new BinaryExpression(expr, (OperatorToken) stream.Token, ParseAndTest());
            }
            return expr;
        }

        /// <summary>
        /// <c>
        ///     not_test:
        ///         'not' not_test | comparison
        /// </c>
        /// </summary>
        private Expression ParseNotTest() {
            if (stream.MaybeEat(TokenType.KeywordNot)) {
                Token      op   = stream.Token;
                Expression expr = ParseNotTest();
                if (expr is UnaryExpression unary
                 && unary.Operator.Type == TokenType.KeywordNot) {
                    Blame(BlameType.DoubleNegationIsMeaningless, op.Span.StartPosition, unary.Operator.Span.EndPosition);
                }
                return new UnaryExpression(op, expr);
            }
            return ParseComparison();
        }

        /// <summary>
        /// <c>
        ///     comparison:
        ///         expr (COMPARISON_OPERATOR expr)*
        /// </c>
        /// </summary>
        private Expression ParseComparison() {
            Expression expr = ParseExpr();
            while (stream.MaybeEat(Spec.ComparisonOperators)) {
                var        op  = (OperatorToken) stream.Token;
                Expression rhs = ParseComparison();
                expr = new BinaryExpression(expr, op, rhs);
            }
            return expr;
        }

        //============================================================
        //============================================================
        //============================================================

        /// <summary>
        /// <c>
        ///     expr:
        ///         xor_expr ('|' xor_expr)*
        ///     xor_expr:
        ///         and_expr ('^' and_expr)*
        ///     and_expr:
        ///         shift_expr ('&' shift_expr)*
        ///     shift_expr:
        ///         arithmetic_expr (('<<' | '>>') arithmetic_expr)*
        ///     arithmetic_expr:
        ///         term (('+'|'-') term)*
        ///     term:
        ///         factor (('*'|'@'|'/'|'%'|'//') factor)*
        /// </c>
        /// </summary>
        private Expression ParseExpr(int precedence = 0) {
            Expression expr = ParseFactor();
            while (true) {
                if (!(stream.Peek is OperatorToken ot)) {
                    return expr;
                }

                int newPrecedence = ot.Properties.Precedence;
                if (newPrecedence >= precedence) {
                    stream.NextToken();
                    // TODO: fix precedences
                    expr = new BinaryExpression(expr, ot, ParseExpr(newPrecedence + 1));
                }
                else {
                    return expr;
                }
            }
        }

        /// <summary>
        /// <c>
        ///     factor:
        ///         ('+'|'-'|'~') factor | power
        /// </c>
        /// </summary>
        private Expression ParseFactor() {
            if (stream.MaybeEat(
                TokenType.OpAdd,
                TokenType.OpSubtract,
                TokenType.OpBitwiseNot
            )) {
                return new UnaryExpression(stream.Token, ParseFactor());
            }
            return ParsePower();
        }

        /// <summary>
        /// <c>
        ///     power:
        ///         atom_expr trailer* ['**' factor]
        /// </c>
        /// </summary>
        private Expression ParsePower() {
            Expression expr = ParseTrailingExpr(ParsePrimaryExpr(), true);
            if (stream.MaybeEat(TokenType.OpPower)) {
                expr = new BinaryExpression(expr, stream.Token as OperatorToken, ParseFactor());
            }
            return expr;
        }

        #endregion
    }
}