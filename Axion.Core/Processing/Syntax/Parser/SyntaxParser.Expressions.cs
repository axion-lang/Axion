using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.Binary;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         expr_stmt ::=
        ///             test_list (
        ///                 (COMPOUND_ASSIGN assign_value) |
        ///                 {'='             assign_value}
        ///             )
        ///         assign_value ::=
        ///             yield_expr | test_list
        ///     </c>
        /// </summary>
        private Expression ParseExpression() {
            Expression expr = ParseTestList();
            if (expr is ErrorExpression || expr is UnaryExpression) {
                return expr;
            }

            if (Stream.MaybeEat(Colon)) {
                TypeName   type  = ParseTypeName();
                Expression value = null;
                if (Stream.MaybeEat(Assign)) {
                    if (Stream.PeekIs(KeywordYield)) {
                        value = ParseYield();
                    }
                    else {
                        value = ParseTestList();
                    }
                }

                if (expr.CannotAssignReason != null) {
                    ReportError(expr.CannotAssignReason, expr);
                }

                return new VarDefinitionExpression(expr, type, value);
            }

            if (Stream.PeekIs(Assign)) {
                return FinishAssignments(expr);
            }

            if (Stream.MaybeEat(Spec.CompoundAssignOperators)) {
                var        op = (SymbolToken) Stream.Token;
                Expression right;
                if (Stream.PeekIs(KeywordYield)) {
                    right = ParseYield();
                }
                else {
                    right = ParseTestList();
                }

                if (expr.CannotAssignReason != null) {
                    ReportError(expr.CannotAssignReason, expr);
                }

                return new CompoundAssignExpression(expr, op, right);
            }

            return expr;
        }

        private Expression FinishAssignments(Expression right) {
            var left = new List<Expression>();
            while (Stream.MaybeEat(Assign)) {
                if (right.CannotAssignReason != null) {
                    ReportError(right.CannotAssignReason, right);
                }

                left.Add(right);

                if (Stream.PeekIs(KeywordYield)) {
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
        ///         yield_expr ::=
        ///             'yield' ['from' test | test_list]
        ///     </c>
        /// </summary>
        private Expression ParseYield() {
            Position start = StartExprOrStmt(KeywordYield).Span.StartPosition;
            // Mark that function as generator.
            // If we're in a generator expr, then we don't have a function yet.
            // g = ((yield i) for i in range(5))
            // In that case, the genexp will mark IsGenerator. 
            FunctionDefinition current = currentFunction;
            if (current != null) {
                current.IsGenerator = true;
            }

            // Parse expr list after yield. This can be:
            // 1) empty, in which case it becomes 'yield None'
            // 2) a single expr
            // 3) multiple expressions, in which case it's wrapped in a tuple.
            Expression yieldExpr;

            var isYieldFrom = false;
            if (Stream.MaybeEat(KeywordFrom)) {
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
        ///         test ::=
        ///             or_test [cond_expr] |
        ///             lambda_def
        ///     </c>
        /// </summary>
        private Expression ParseTestExpr() {
            Expression expr = ParseOrExpr();
            if (Stream.PeekIs(KeywordIf, KeywordUnless)
                && Stream.Token.Type != Newline) {
                expr = ParseConditionExpr(expr);
            }

            return expr;
        }

        #region Expressions with priority (from lower to higher precedence)

        /// <summary>
        ///     <c>
        ///         cond_test ::=
        ///             'if' or_test ['else' test]
        ///     </c>
        /// </summary>
        private Expression ParseConditionExpr(Expression trueExpr) {
            bool invert = Stream.MaybeEat(KeywordUnless);
            if (!invert) {
                Stream.Eat(KeywordIf);
            }

            Expression expr      = ParseOrExpr();
            Expression falseExpr = null;
            if (Stream.MaybeEat(KeywordElse)) {
                falseExpr = ParseTestExpr();
            }
            else if (Stream.MaybeEat(KeywordUnless)) {
                falseExpr = trueExpr;
                trueExpr  = ParseTestExpr();
            }

            return invert
                ? new ConditionalExpression(expr, falseExpr, trueExpr)
                : new ConditionalExpression(expr, trueExpr, falseExpr);
        }

        /// <summary>
        ///     <c>
        ///         or_test ::=
        ///             and_test ('or' and_test)*
        ///     </c>
        /// </summary>
        private Expression ParseOrExpr() {
            Expression expr = ParseAndExpr();
            while (Stream.MaybeEat(KeywordOr)) {
                expr = new OrExpression(expr, ParseAndExpr());
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         and_test ::=
        ///             not_test ('and' not_test)*
        ///     </c>
        /// </summary>
        private Expression ParseAndExpr() {
            Expression expr = ParseNotExpr();
            while (Stream.MaybeEat(KeywordAnd)) {
                expr = new AndExpression(expr, ParseAndExpr());
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         not_test ::=
        ///             'not' not_test | comparison
        ///     </c>
        /// </summary>
        private Expression ParseNotExpr() {
            if (Stream.MaybeEat(KeywordNot)) {
                var        op   = (OperatorToken) Stream.Token;
                Expression expr = ParseNotExpr();
                if (expr is UnaryExpression unary
                    && unary.Operator.Type == KeywordNot) {
                    Blame(
                        BlameType.DoubleNegationIsMeaningless,
                        op.Span.StartPosition,
                        unary.Operator.Span.EndPosition
                    );
                }

                op.Properties.InputSide = InputSide.Left;
                return new UnaryExpression(op, expr);
            }

            return ParseCompareExpr();
        }

        /// <summary>
        ///     <c>
        ///         comparison ::=
        ///             expr (COMPARISON_OPERATOR expr)*
        ///     </c>
        /// </summary>
        private Expression ParseCompareExpr() {
            Expression expr = ParsePriorityExpr();
            while (Stream.MaybeEat(Spec.ComparisonOperators)) {
                var op = (OperatorToken) Stream.Token;
                if (op.Type == OpLessThan) { }

                Expression rhs = ParseCompareExpr();
                expr = new BinaryExpression(expr, op, rhs);
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         expr ::=
        ///             xor_expr ('|' xor_expr)*
        ///         xor_expr ::=
        ///             and_expr ('^' and_expr)*
        ///         and_expr ::=
        ///             shift_expr ('&amp;' shift_expr)*
        ///         shift_expr ::=
        ///             arithmetic_expr (('&lt;&lt;' | '>>') arithmetic_expr)*
        ///         arithmetic_expr ::=
        ///             term (('+' | '-') term)*
        ///         term ::=
        ///             factor (('*' | '@' | '/' | '%' | '//') factor)*
        ///     </c>
        /// </summary>
        private Expression ParsePriorityExpr(int precedence = 0) {
            Expression expr = ParseFactorExpr();
            while (true) {
                if (!(Stream.Peek is OperatorToken ot)) {
                    return expr;
                }

                int newPrecedence = ot.Properties.Precedence;
                if (newPrecedence >= precedence) {
                    Stream.NextToken();
                    // TODO: fix precedences
                    expr = new BinaryExpression(expr, ot, ParsePriorityExpr(newPrecedence + 1));
                }
                else {
                    return expr;
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         factor ::=
        ///             ('+' | '-' | '~' | '++' | '--') factor | power
        ///     </c>
        /// </summary>
        private Expression ParseFactorExpr() {
            if (Stream.MaybeEat(OpAdd, OpSubtract, OpBitwiseNot, OpIncrement, OpDecrement)) {
                var op = (OperatorToken) Stream.Token;
                op.Properties.InputSide = InputSide.Left;
                return new UnaryExpression(op, ParseFactorExpr());
            }

            return ParsePowerExpr();
        }

        /// <summary>
        ///     <c>
        ///         power ::=
        ///             atom_expr trailer* ['**' factor]
        ///     </c>
        /// </summary>
        private Expression ParsePowerExpr() {
            Expression expr = ParseTrailingExpr(ParsePrimaryExpr(), true);
            if (Stream.MaybeEat(OpPower)) {
                expr = new BinaryExpression(expr, Stream.Token as OperatorToken, ParseFactorExpr());
            }

            return expr;
        }

        #endregion

        /// <summary>
        ///     <c>
        ///         trailer ::=
        ///             (expr ('++' | '--'))
        ///             | call
        ///             | '[' subscription_list ']'
        ///             | member
        ///         call ::=
        ///             '(' [args_list | comprehension_iterator] ')'
        ///         member ::=
        ///             '.' ID
        ///     </c>
        /// </summary>
        private Expression ParseTrailingExpr(Expression result, bool allowGeneratorExpression) {
            while (true) {
                switch (Stream.Peek.Type) {
                    case OpIncrement:
                    case OpDecrement: {
                        Stream.NextToken();
                        var op = (OperatorToken) Stream.Token;
                        op.Properties.InputSide = InputSide.Right;
                        result = new UnaryExpression(op, result);
                        break;
                    }
                    case LeftParenthesis: {
                        if (!allowGeneratorExpression) {
                            // TODO: check
                            return result;
                        }

                        Stream.NextToken();
                        Arg[] args = FinishGeneratorOrArgList();
                        result = new CallExpression(result, args ?? new Arg[0], tokenEnd);
                        break;
                    }
                    case LeftBracket: {
                        result = new IndexExpression(result, ParseSubscriptList());
                        break;
                    }
                    case Dot: {
                        Stream.NextToken();
                        result = new MemberExpression(result, ParseName());
                        break;
                    }
                    default: {
                        if (Stream.PeekIs(Spec.ConstantValueTypes)) {
                            // abc 1, abc "", abc 1L, abc 0j
                            ReportError(Spec.ERR_InvalidStatement, Stream.Peek);
                            return Error();
                        }

                        return result;
                    }
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         subscriptions_list ::=
        ///             subscription {',' subscription} [',']
        ///         subscription ::=
        ///             expr | slice
        ///         slice ::=
        ///             [expr] ':' [expr] [':' [expr]]
        ///     </c>
        /// </summary>
        private Expression ParseSubscriptList() {
            Stream.Eat(LeftBracket);

            bool trailingComma;
            var  expressions = new List<Expression>();
            do {
                Expression expr = null;
                if (!Stream.PeekIs(Colon)) {
                    expr = ParseTestExpr();
                }

                if (Stream.MaybeEat(Colon)) {
                    Expression stop = null;
                    if (!Stream.PeekIs(Colon, Comma, RightBracket)) {
                        // [?:val:?]
                        stop = ParseTestExpr();
                    }

                    Expression step = null;
                    if (Stream.MaybeEat(Colon)
                        && !Stream.PeekIs(Comma, RightBracket)) {
                        // [?:?:val]
                        step = ParseTestExpr();
                    }

                    return new SliceExpression(expr, stop, step);
                }

                expressions.Add(expr);
                trailingComma = Stream.MaybeEat(Comma);
            } while (trailingComma && !Stream.PeekIs(RightBracket));

            Stream.Eat(RightBracket);
            return MakeTupleOrExpr(expressions, trailingComma, true);
        }
        
        /// <summary>
        ///     <c>
        ///         name ::=
        ///             ID
        ///     </c>
        /// </summary>
        private NameExpression ParseName() {
            NameExpression name = null;
            if (Stream.Eat(Identifier)) {
                name = new NameExpression(Stream.Token);
            }

            return name;
        }

        /// <summary>
        ///     <c>
        ///         name ::=
        ///             name {member}
        ///     </c>
        /// </summary>
        private Expression ParseQualifiedName() {
            Expression name = null;
            if (Stream.Eat(Identifier)) {
                name = new NameExpression(Stream.Token);
            }

            // qualifiers
            while (Stream.MaybeEat(Dot)) {
                NameExpression qualifier = null;
                if (Stream.Eat(Identifier)) {
                    qualifier = new NameExpression(Stream.Token);
                }

                name = new MemberExpression(name, qualifier);
            }

            return name;
        }
    }
}