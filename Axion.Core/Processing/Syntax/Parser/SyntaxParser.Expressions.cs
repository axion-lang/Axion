using System.Collections.Generic;
using System.Diagnostics;
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
            if (expr is ErrorExpression
                || expr is UnaryExpression) {
                return expr;
            }

            if (stream.MaybeEat(Colon)) {
                TypeName   type  = ParseTypeName();
                Expression value = null;
                if (stream.MaybeEat(Assign)) {
                    value = stream.PeekIs(KeywordYield)
                        ? ParseYield()
                        : ParseTestList();
                }

                if (expr.CannotAssignReason != null) {
                    unit.ReportError(expr.CannotAssignReason, expr);
                }

                return new VarDefinitionExpression(expr, type, value);
            }

            if (stream.PeekIs(Assign)) {
                return FinishAssignments(expr);
            }

            if (stream.MaybeEat(Spec.CompoundAssignOperators)) {
                var op = (SymbolToken) stream.Token;
                Expression right = stream.PeekIs(KeywordYield)
                    ? ParseYield()
                    : ParseTestList();

                if (expr.CannotAssignReason != null) {
                    unit.ReportError(expr.CannotAssignReason, expr);
                }

                return new CompoundAssignExpression(expr, op, right);
            }

            return expr;
        }

        private Expression FinishAssignments(Expression right) {
            var left = new List<Expression>();
            while (stream.MaybeEat(Assign)) {
                if (right.CannotAssignReason != null) {
                    unit.ReportError(right.CannotAssignReason, right);
                }

                left.Add(right);

                right = stream.PeekIs(KeywordYield)
                    ? ParseYield()
                    : ParseTestList();
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
            Token start = StartExprOrStmt(KeywordYield);
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
            if (stream.MaybeEat(KeywordFrom)) {
                yieldExpr   = ParseTestExpr();
                isYieldFrom = true;
            }
            else {
                yieldExpr = ParseExpression() ?? new ConstantExpression(KeywordNil);
            }

            return new YieldExpression(yieldExpr, isYieldFrom, start, stream.Token);
        }

        /// <summary>
        ///     <c>
        ///         test ::=
        ///             priority_expr [cond_expr]
        ///             | lambda_def
        ///     </c>
        /// </summary>
        private Expression ParseTestExpr() {
            Expression expr = ParseOperation();
            if (stream.PeekIs(KeywordIf, KeywordUnless)
                && stream.Token.Type != Newline) {
                expr = ParseConditionExpr(expr);
            }

            return expr;
        }

        #region Expressions with priority (from lower to higher precedence)

        /// <summary>
        ///     <c>
        ///         cond_expr ::=
        ///             'if' priority_expr ['else' test]
        ///     </c>
        /// </summary>
        private Expression ParseConditionExpr(Expression trueExpr) {
            bool invert = stream.MaybeEat(KeywordUnless);
            if (!invert) {
                stream.Eat(KeywordIf);
            }

            Expression expr      = ParseOperation();
            Expression falseExpr = null;
            if (stream.MaybeEat(KeywordElse)) {
                falseExpr = ParseTestExpr();
            }
            else if (stream.MaybeEat(KeywordUnless)) {
                falseExpr = trueExpr;
                trueExpr  = ParseTestExpr();
            }

            return invert
                ? new ConditionalExpression(expr, falseExpr, trueExpr)
                : new ConditionalExpression(expr, trueExpr, falseExpr);
        }

        private Expression ParseOperation() {
            return ParsePriorityExpr();
        }

        /// <summary>
        ///     <c>
        ///         priority_expr ::=
        ///             factor OPERATOR priority_expr
        ///     </c>
        /// </summary>
        private Expression ParsePriorityExpr(int precedence = 0) {
            Expression expr = ParseFactorExpr();
            while (true) {
                if (!(stream.Peek is OperatorToken op)) {
                    return expr;
                }

                int newPrecedence = op.Properties.Precedence;
                if (newPrecedence >= precedence) {
                    stream.NextToken();
                    expr = new BinaryExpression(
                        expr,
                        op,
                        ParsePriorityExpr(newPrecedence + 1)
                    );
                }
                else {
                    return expr;
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         factor ::=
        ///             ('+' | '-' | '~' | 'not' | '++' | '--') factor | power
        ///     </c>
        /// </summary>
        private Expression ParseFactorExpr() {
            if (stream.MaybeEat(Spec.FactorOperators)) {
                var op = (OperatorToken) stream.Token;
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
            if (stream.MaybeEat(OpPower)) {
                expr = new BinaryExpression(expr, stream.Token as OperatorToken, ParseFactorExpr());
            }

            return expr;
        }

        #endregion

        /// <summary>
        ///     <c>
        ///         trailer ::=
        ///             (expr ('++' | '--'))
        ///             | ('[' subscription_list ']')
        ///             | call
        ///             | member
        ///         call ::=
        ///             '(' [args_list | comprehension_iterator] ')'
        ///         member ::=
        ///             '.' ID
        ///     </c>
        /// </summary>
        private Expression ParseTrailingExpr(Expression result, bool allowGeneratorExpression) {
            while (true) {
                switch (stream.Peek.Type) {
                    case OpIncrement:
                    case OpDecrement: {
                        stream.NextToken();
                        var op = (OperatorToken) stream.Token;
                        op.Properties.InputSide = InputSide.Right;
                        result                  = new UnaryExpression(op, result);
                        break;
                    }
                    case LeftParenthesis: {
                        if (!allowGeneratorExpression) {
                            return result;
                        }

                        stream.NextToken();
                        Arg[] args = FinishGeneratorOrArgList();
                        result = new CallExpression(result, args ?? new Arg[0], stream.Token);
                        break;
                    }
                    case LeftBracket: {
                        result = new IndexExpression(result, ParseSubscriptList());
                        break;
                    }
                    case Dot: {
                        stream.NextToken();
                        result = new MemberExpression(result, ParseName());
                        break;
                    }
                    default: {
                        if (stream.PeekIs(Spec.ConstantValueTypes)) {
                            // abc 1, abc "", abc 1L, abc 0j
                            unit.ReportError(Spec.ERR_InvalidStatement, stream.Peek);
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
            stream.Eat(LeftBracket);

            bool trailingComma;
            var  expressions = new List<Expression>();
            do {
                Expression expr = null;
                if (!stream.PeekIs(Colon)) {
                    expr = ParseTestExpr();
                }

                if (stream.MaybeEat(Colon)) {
                    Expression stop = null;
                    if (!stream.PeekIs(Colon, Comma, RightBracket)) {
                        stop = ParseTestExpr();
                    }

                    Expression step = null;
                    if (stream.MaybeEat(Colon)
                        && !stream.PeekIs(Comma, RightBracket)) {
                        step = ParseTestExpr();
                    }

                    return new SliceExpression(expr, stop, step);
                }

                expressions.Add(expr);
                trailingComma = stream.MaybeEat(Comma);
            } while (trailingComma && !stream.PeekIs(RightBracket));

            stream.Eat(RightBracket);
            return MakeTupleOrExpr(expressions, trailingComma, true);
        }

        #region Name

        /// <summary>
        ///     <c>
        ///         name ::=
        ///             ID
        ///     </c>
        /// </summary>
        private NameExpression ParseName() {
            NameExpression name = null;
            if (stream.Eat(Identifier)) {
                name = new NameExpression((IdentifierToken) stream.Token);
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
            if (stream.Eat(Identifier)) {
                name = new NameExpression((IdentifierToken) stream.Token);
            }

            // qualifiers
            while (stream.MaybeEat(Dot)) {
                NameExpression qualifier = null;
                if (stream.Eat(Identifier)) {
                    qualifier = new NameExpression((IdentifierToken) stream.Token);
                }

                name = new MemberExpression(name, qualifier);
            }

            return name;
        }

        #endregion
    }
}