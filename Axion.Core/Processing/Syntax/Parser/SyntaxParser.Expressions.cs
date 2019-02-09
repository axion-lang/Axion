using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         name:
        ///         ID ('.' ID)*
        ///     </c>
        /// </summary>
        private NameExpression ParseName(bool allowQualified = false) {
            var nameParts = new List<Token>();
            do {
                if (stream.Eat(TokenType.Identifier)) {
                    nameParts.Add(stream.Token);
                }
            } while (allowQualified && stream.MaybeEat(TokenType.Dot));
            return new NameExpression(nameParts.ToArray());
        }

        /// <summary>
        ///     <c>
        ///         expr_stmt:
        ///         test_list (
        ///         (AUG_ASSIGN assign_value)
        ///         | ('='        assign_value)*
        ///         )
        ///         assign_value:
        ///         yield_expr | test_list
        ///     </c>
        /// </summary>
        private Expression ParseExpression() {
            Expression expr = ParseTestList();
            if (expr is ErrorExpression) {
                stream.NextToken();
            }
            if (stream.MaybeEat(TokenType.Colon)) {
                TypeName   type  = ParseTypeName();
                Expression value = null;
                if (stream.MaybeEat(TokenType.Assign)) {
                    if (stream.PeekIs(TokenType.KeywordYield)) {
                        value = ParseYield();
                    }
                    else {
                        value = ParseTestList();
                    }
                }
                return new VarDefinitionExpression(expr, type, value);
            }
            if (stream.PeekIs(TokenType.Assign)) {
                expr = FinishAssignments(expr);
            }
            if (stream.MaybeEat(Spec.AugmentedAssignOperators)) {
                var        op = (SymbolToken) stream.Token;
                Expression right;
                if (stream.PeekIs(TokenType.KeywordYield)) {
                    right = ParseYield();
                }
                else {
                    right = ParseTestList();
                }
                if (expr.CannotAugAssignReason != null) {
                    ReportError(expr.CannotAugAssignReason, expr);
                }
                expr = new AugmentedAssignExpression(expr, op, right);
            }
            return expr;
        }

        private Expression FinishAssignments(Expression right) {
            var left = new List<Expression>();
            while (stream.MaybeEat(TokenType.Assign)) {
                if (right.CannotAssignReason != null) {
                    ReportError(right.CannotAssignReason, right);
                }
                left.Add(right);

                if (stream.PeekIs(TokenType.KeywordYield)) {
                    right = ParseYield();
                }
                else {
                    right = ParseTestList();
                }
            }

            Debug.Assert(left.Count > 0);
            return new AssignmentExpression(left.ToArray(), right);
        }

        /// <summary>
        ///     <c>
        ///         yield_expr:
        ///         'yield' ['from' test | test_list]
        ///     </c>
        /// </summary>
        private Expression ParseYield() {
            Position start = StartExprOrStmt(TokenType.KeywordYield).Span.StartPosition;
            // Mark that function as generator.
            // If we're in a generator expr, then we don't have a function yet.
            // g = ((yield i) for i in range(5))
            // In that case, the genexp will mark IsGenerator. 
            FunctionDefinition current = ast.CurrentFunction;
            if (current != null) {
                current.IsGenerator = true;
            }

            // Parse expr list after yield. This can be:
            // 1) empty, in which case it becomes 'yield None'
            // 2) a single expr
            // 3) multiple expressions, in which case it's wrapped in a tuple.
            Expression yieldExpr;

            var isYieldFrom = false;
            if (stream.MaybeEat(TokenType.KeywordFrom)) {
                yieldExpr = ParseTestExpr();
                yieldExpr.MarkPosition(start, tokenEnd);
                isYieldFrom = true;
            }
            else {
                yieldExpr = ParseExpression() ?? new ConstantExpression(null, start, tokenEnd);
            }
            return new YieldExpression(yieldExpr, isYieldFrom, start, tokenEnd);
        }

        /// <summary>
        ///     <c>
        ///         test:
        ///         or_test [cond_expr] |
        ///         lambda_def
        ///     </c>
        /// </summary>
        private Expression ParseTestExpr() {
            Expression expr = ParseOrExpr();
            if (stream.PeekIs(TokenType.KeywordIf, TokenType.KeywordUnless) && stream.Token.Type != TokenType.Newline) {
                expr = ParseConditionExpr(expr);
            }
            return expr;
        }

        /// <summary>
        ///     <c>
        ///         trailer:
        ///         call
        ///         | '[' subscription_list ']'
        ///         | member
        ///         call:
        ///         '(' [args_list | comprehension_iterator] ')'
        ///         member:
        ///         '.' ID
        ///     </c>
        /// </summary>
        private Expression ParseTrailingExpr(Expression result, bool allowGeneratorExpression) {
            while (true) {
                if (stream.PeekIs(TokenType.LeftParenthesis)) {
                    if (!allowGeneratorExpression) {
                        // TODO: check
                        return result;
                    }

                    stream.NextToken();
                    Arg[] args = FinishGeneratorOrArgList();
                    result = new CallExpression(result, args ?? new Arg[0], tokenEnd);
                }
                else if (stream.PeekIs(TokenType.LeftBracket)) {
                    result = new IndexExpression(result, ParseSubscriptList());
                }
                else if (stream.MaybeEat(TokenType.Dot)) {
                    result = new MemberExpression(result, ParseName());
                }
                else if (stream.PeekIs(Spec.ConstantValueTypes)) {
                    // abc 1, abc "", abc 1L, abc 0j
                    ReportError(Spec.ERR_InvalidStatement, stream.Peek);
                    return Error();
                }
                else {
                    return result;
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         subscriptions_list:
        ///         subscription (',' subscription)* [',']
        ///         subscription:
        ///         expr | slice
        ///         slice:
        ///         [expr] ':' [expr] [ ':' [expr] ]
        ///     </c>
        /// </summary>
        private Expression ParseSubscriptList() {
            stream.Eat(TokenType.LeftBracket);
            bool trailingComma;

            var expressions = new List<Expression>();
            do {
                Expression expr = null;
                if (!stream.PeekIs(TokenType.Colon)) {
                    expr = ParseTestExpr();
                }
                if (stream.MaybeEat(TokenType.Colon)) {
                    expr = FinishSlice(expr);
                }
                expressions.Add(expr);
                trailingComma = stream.MaybeEat(TokenType.Comma);
            } while (trailingComma && !stream.PeekIs(TokenType.RightBracket));
            stream.Eat(TokenType.RightBracket);

            Expression FinishSlice(Expression start) {
                Expression stop = null;
                if (stream.MaybeEat(TokenType.Colon) && !stream.PeekIs(TokenType.Comma, TokenType.RightBracket)) {
                    // x[?:val:?]
                    stop = ParseTestExpr();
                }
                Expression step = null;
                if (stream.MaybeEat(TokenType.Colon) && !stream.PeekIs(TokenType.Comma, TokenType.RightBracket)) {
                    // x[?:?:val]
                    step = ParseTestExpr();
                }

                return new SliceExpression(start, stop, step);
            }

            return MakeTupleOrExpr(expressions, trailingComma, true);
        }

        #region Arithmetic precedence-based expressions

        /// <summary>
        ///     <c>
        ///         cond_test:
        ///         'if' or_test ['else' test]
        ///     </c>
        /// </summary>
        private Expression ParseConditionExpr(Expression trueExpr) {
            bool invert = stream.MaybeEat(TokenType.KeywordUnless);
            if (!invert) {
                stream.Eat(TokenType.KeywordIf);
            }
            Expression expr      = ParseOrExpr();
            Expression falseExpr = null;
            if (stream.MaybeEat(TokenType.KeywordElse)) {
                falseExpr = ParseTestExpr();
            }
            else if (stream.MaybeEat(TokenType.KeywordUnless)) {
                falseExpr = trueExpr;
                trueExpr  = ParseTestExpr();
            }
            return invert
                       ? new ConditionalExpression(expr, falseExpr, trueExpr)
                       : new ConditionalExpression(expr, trueExpr,  falseExpr);
        }

        /// <summary>
        ///     <c>
        ///         or_test:
        ///         and_test ('or' and_test)*
        ///     </c>
        /// </summary>
        private Expression ParseOrExpr() {
            Expression expr = ParseAndExpr();
            while (stream.MaybeEat(TokenType.KeywordOr)) {
                expr = new BinaryExpression(expr, (OperatorToken) stream.Token, ParseAndExpr());
            }
            return expr;
        }

        /// <summary>
        ///     <c>
        ///         and_test:
        ///         not_test ('and' not_test)*
        ///     </c>
        /// </summary>
        private Expression ParseAndExpr() {
            Expression expr = ParseNotExpr();
            while (stream.MaybeEat(TokenType.KeywordAnd)) {
                expr = new BinaryExpression(expr, (OperatorToken) stream.Token, ParseAndExpr());
            }
            return expr;
        }

        /// <summary>
        ///     <c>
        ///         not_test:
        ///         'not' not_test | comparison
        ///     </c>
        /// </summary>
        private Expression ParseNotExpr() {
            if (stream.MaybeEat(TokenType.KeywordNot)) {
                Token      op   = stream.Token;
                Expression expr = ParseNotExpr();
                if (expr is UnaryExpression unary && unary.Operator.Type == TokenType.KeywordNot) {
                    Blame(
                        BlameType.DoubleNegationIsMeaningless,
                        op.Span.StartPosition,
                        unary.Operator.Span.EndPosition
                    );
                }
                return new UnaryExpression(op, expr);
            }
            return ParseCompareExpr();
        }

        /// <summary>
        ///     <c>
        ///         comparison:
        ///         expr (COMPARISON_OPERATOR expr)*
        ///     </c>
        /// </summary>
        private Expression ParseCompareExpr() {
            Expression expr = ParseExpr();
            while (stream.MaybeEat(Spec.ComparisonOperators)) {
                var op = (OperatorToken) stream.Token;
                if (op.Type == TokenType.OpLessThan) {
                }
                Expression rhs = ParseCompareExpr();
                expr = new BinaryExpression(expr, op, rhs);
            }
            return expr;
        }

        //============================================================
        //============================================================
        //============================================================

        /// <summary>
        ///     <c>
        ///         expr:
        ///         xor_expr ('|' xor_expr)*
        ///         xor_expr:
        ///         and_expr ('^' and_expr)*
        ///         and_expr:
        ///         shift_expr ('&' shift_expr)*
        ///         shift_expr:
        ///         arithmetic_expr (('<<' | '>>') arithmetic_expr)*
        ///         arithmetic_expr:
        ///         term (('+'|'-') term)*
        ///         term:
        ///         factor (('*'|'@'|'/'|'%'|'//') factor)*
        ///     </c>
        /// </summary>
        private Expression ParseExpr(int precedence = 0) {
            Expression expr = ParseFactorExpr();
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
        ///     <c>
        ///         factor:
        ///         ('+'|'-'|'~') factor | power
        ///     </c>
        /// </summary>
        private Expression ParseFactorExpr() {
            if (stream.MaybeEat(TokenType.OpAdd, TokenType.OpSubtract, TokenType.OpBitwiseNot)) {
                return new UnaryExpression(stream.Token, ParseFactorExpr());
            }
            return ParsePowerExpr();
        }

        /// <summary>
        ///     <c>
        ///         power:
        ///         atom_expr trailer* ['**' factor]
        ///     </c>
        /// </summary>
        private Expression ParsePowerExpr() {
            Expression expr = ParseTrailingExpr(ParsePrimaryExpr(), true);
            if (stream.MaybeEat(TokenType.OpPower)) {
                expr = new BinaryExpression(expr, stream.Token as OperatorToken, ParseFactorExpr());
            }
            return expr;
        }

        #endregion
    }
}