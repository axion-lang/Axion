using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         expr_list:
    ///             expr {',' expr}
    ///         preglobal_list:
    ///             preglobal_expr {',' preglobal_expr}
    ///         simple_name_list:
    ///             simple_name_expr {',' simple_name_expr}
    ///     </c>
    /// </summary>
    public abstract class Expression : SyntaxTreeNode {
        protected Expression(SyntaxTreeNode parent) : base(parent) { }
        protected Expression() { }

        /// <summary>
        ///     <c>
        ///         primary
        ///             : name
        ///             | await_expr
        ///             | yield_expr
        ///             | type_initializer_expr
        ///             | parenthesis_expr
        ///             | list_expr
        ///             | hash_collection
        ///             | CONSTANT
        ///     </c>
        /// </summary>
        internal static Expression ParsePrimaryExpr(SyntaxTreeNode parent) {
            Expression value;
            switch (parent.Peek.Type) {
                case Identifier: {
                    value = NameExpression.ParseName(parent);
                    break;
                }

                case KeywordAwait: {
                    value = new AwaitExpression(parent);
                    break;
                }

                case KeywordYield: {
                    value = new YieldExpression(parent);
                    break;
                }

                case KeywordNew: {
                    value = new TypeInitializerExpression(parent);
                    break;
                }

                case KeywordFn: {
                    value = new LambdaExpression(parent);
                    break;
                }

                case OpenBracket: {
                    value = new ListInitializerExpression(parent);
                    break;
                }

                case OpenBrace: {
                    value = new BraceCollectionExpression(parent);
                    break;
                }

                case OpenParenthesis: {
                    Token start = parent.Token;
                    parent.Eat(OpenParenthesis);
                    // empty tuple
                    if (parent.MaybeEat(CloseParenthesis)) {
                        return new TupleExpression(parent, start, parent.Token);
                    }

                    value = ParseMultiple(parent, parens: true);
                    parent.Eat(CloseParenthesis);
                    break;
                }

                default: {
                    parent.Move();
                    if (Spec.Constants.Contains(parent.Token.Type)) {
                        // TODO add pre-concatenation of literals
                        value = new ConstantExpression(parent);
                    }
                    else {
                        value = new ErrorExpression(parent);
                    }

                    break;
                }
            }

            return value;
        }

        /// <summary>
        ///     <c>
        ///         extended:
        ///             (pipeline | { member | call_expr | index_expr })
        ///             ['++' | '--']
        ///         pipeline:
        ///             primary {'|>' primary }
        ///     </c>
        /// </summary>
        internal static Expression ParseExtendedExpr(SyntaxTreeNode parent) {
            Expression value = ParsePrimaryExpr(parent);
            if (parent.MaybeEat(RightPipeline)) {
                do {
                    value = new FunctionCallExpression(
                        parent,
                        ParseExtendedExprInternal(parent, ParsePrimaryExpr(parent)),
                        new CallArgument(parent, value)
                    );
                } while (parent.MaybeEat(RightPipeline));

                return value;
            }

            return ParseExtendedExprInternal(parent, value);
        }

        private static Expression ParseExtendedExprInternal(
            SyntaxTreeNode parent,
            Expression     value
        ) {
            while (true) {
                if (parent.Peek.Is(Dot)) {
                    value = new MemberAccessExpression(parent, value);
                }
                else if (parent.Peek.Is(OpenParenthesis)) {
                    value = new FunctionCallExpression(parent, value, true);
                }
                else if (parent.Peek.Is(OpenBracket)) {
                    value = new IndexerExpression(parent, value);
                }
                else {
                    break;
                }
            }

            if (parent.MaybeEat(OpIncrement, OpDecrement)) {
                var op = (OperatorToken) value.Token;
                op.Properties.InputSide = InputSide.Right;
                value                   = new UnaryOperationExpression(parent, op, value);
            }

            return value;
        }

        /// <summary>
        ///     <c>
        ///         unary_left:
        ///             (UNARY_LEFT_OPERATOR unary_left) | trailer
        ///     </c>
        /// </summary>
        internal static Expression ParseUnaryLeftExpr(SyntaxTreeNode parent) {
            if (parent.MaybeEat(Spec.UnaryLeftOperators)) {
                var op = (OperatorToken) parent.Token;
                op.Properties.InputSide = InputSide.Left;
                return new UnaryOperationExpression(parent, op, ParseUnaryLeftExpr(parent));
            }

            return ParseExtendedExpr(parent);
        }

        /// <summary>
        ///     <c>
        ///         operation_expr:
        ///             factor OPERATOR priority_expr
        ///     </c>
        /// </summary>
        internal static Expression ParseOperation(SyntaxTreeNode parent, int precedence = 0) {
            Expression expr = ParseUnaryLeftExpr(parent);
            while (!parent.Peek.Is(Spec.AssignmentOperators)
                   && parent.Peek is OperatorToken op
                   && op.Properties.Precedence >= precedence) {
                parent.Move();
                expr = new BinaryOperationExpression(
                    parent,
                    expr,
                    op,
                    ParseOperation(parent, op.Properties.Precedence + 1)
                );
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         preglobal_expr:
        ///             operation_expr [ASSIGN_OPERATOR preglobal_expr]
        ///     </c>
        ///     assignment operators
        ///     RIGHT to LEFT
        /// </summary>
        internal static Expression ParsePreGlobalExpr(SyntaxTreeNode parent) {
            Expression expr = ParseOperation(parent);
            if (parent.Peek.Is(KeywordIf, KeywordUnless) && parent.Token.Type != Newline) {
                expr = new ConditionalExpression(parent, expr);
            }

            if (parent.MaybeEat(Spec.AssignmentOperators)) {
                var op = (OperatorToken) parent.Token;
                expr = new BinaryOperationExpression(
                    parent,
                    expr,
                    op,
                    ParsePreGlobalExpr(parent)
                );
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         assign_expr
        ///         | (['let'] assignable
        ///            [':' type]
        ///            ['=' (assign_value)])
        ///     </c>
        /// </summary>
        internal static Expression ParseGlobalExpr(SyntaxTreeNode parent) {
            bool isImmutable = parent.MaybeEat(KeywordLet);

            Expression expr = ParseMultiple(parent, ParsePreGlobalExpr);

            // ['let'] name '=' expr
            if (expr is BinaryOperationExpression bin
                && bin.Left is SimpleNameExpression name
                && bin.Operator.Is(OpAssign)
                && !bin.ParentBlock.HasVariable(name)) {
                return new VariableDefinitionExpression(
                    parent,
                    bin.Left,
                    null,
                    bin.Right,
                    isImmutable
                );
            }

            if (!parent.Peek.Is(Colon)) {
                return expr;
            }

            if (!Spec.VariableLeftExprs.Contains(expr.GetType())) {
                parent.Unit.Blame(BlameType.ThisExpressionTargetIsNotAssignable, expr);
            }

            TypeName   type  = null;
            Expression value = null;
            if (parent.MaybeEat(Colon)) {
                type = TypeName.ParseTypeName(parent);
            }

            if (parent.MaybeEat(OpAssign)) {
                value = ParseMultiple(parent, expectedTypes: Spec.PreGlobalExprs);
            }

            return new VariableDefinitionExpression(parent, expr, type, value, isImmutable);
        }

        /// <summary>
        ///     <c>
        ///         ['('] %expr {',' %expr} [')']
        ///     </c>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal static Expression ParseMultiple(
            SyntaxTreeNode                   parent,
            Func<SyntaxTreeNode, Expression> parserFunc = null,
            bool                             parens     = false,
            params Type[]                    expectedTypes
        ) {
            if (expectedTypes.Length == 0 || parserFunc == ParseGlobalExpr) {
                expectedTypes = Spec.GlobalExprs;
            }

            parserFunc??=ParseGlobalExpr;
            var list = new NodeList<Expression>(parent) {
                parserFunc(parent)
            };

            if (parens && parent.Peek.Is(CloseParenthesis)) {
                return list[0];
            }

            // tuple
            if (parent.MaybeEat(Comma)) {
                bool trailingComma;
                do {
                    list.Add(parserFunc(parent));
                    trailingComma = parent.MaybeEat(Comma);
                } while (trailingComma);
            }
            // generator | comprehension
            else if (parent.Peek.Is(KeywordFor) && parent.Token.Type != Newline) {
                list[0] = new ForComprehension(parent, list[0]);
                if (parens) {
                    list[0] = new GeneratorExpression(parent, (ForComprehension) list[0]);
                }
            }

            CheckType(list, expectedTypes);

            if (parens && list.Count == 1) {
                return new ParenthesizedExpression(list[0]);
            }

            return MaybeTuple(parent, list);
        }

        internal static Expression MaybeTuple(
            SyntaxTreeNode       parent,
            NodeList<Expression> expressions
        ) {
            if (expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpression(parent, expressions);
        }

        /// <summary>
        ///     Checks if every expression in collection
        ///     belong to any of <paramref name="expectedTypes"/>.
        /// </summary>
        internal static void CheckType(
            IEnumerable<Expression> expressions,
            params Type[]           expectedTypes
        ) {
            if (expectedTypes.Length == 0) {
                return;
            }

            foreach (Expression e in expressions) {
                CheckType(e);
            }
        }

        /// <summary>
        ///     Checks if expression
        ///     belongs to any of <paramref name="expectedTypes"/>.
        /// </summary>
        internal static void CheckType(
            Expression    expr,
            params Type[] expectedTypes
        ) {
            if (expectedTypes.Length == 0) {
                return;
            }

            Type itemType = expr.GetType();
            if (expectedTypes.Contains(itemType)) {
                return;
            }

            if (Spec.ExprGroupNames.ContainsKey(expectedTypes)) {
                expr.Unit.ReportError(
                    "Expected "
                    + Spec.ExprGroupNames[expectedTypes]
                    + ", got "
                    + Utilities.GetExprFriendlyName(itemType.Name),
                    expr
                );
            }
            else {
                expr.Unit.ReportError(
                    "Expected "
                    + Utilities.GetExprFriendlyName(expectedTypes[0].Name)
                    + ", got "
                    + Utilities.GetExprFriendlyName(itemType.Name),
                    expr
                );
            }
        }
    }
}