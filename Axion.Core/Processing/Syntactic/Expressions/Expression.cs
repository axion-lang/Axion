using System;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public abstract class Expression : SyntaxTreeNode {
        protected Expression(SyntaxTreeNode parent) : base(parent) { }
        protected Expression() { }

        /// <summary>
        ///     <c>
        ///         primary
        ///             : ID
        ///             | CONSTANT
        ///             | await_expr
        ///             | new_expr
        ///             | parenthesis_expr
        ///             | list_expr
        ///             | hash_collection
        ///     </c>
        /// </summary>
        internal static Expression ParsePrimary(SyntaxTreeNode parent) {
            Expression value;
            switch (parent.Peek.Type) {
                case Identifier: {
                    value = new NameExpression(parent);
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

                case OpenParenthesis: {
                    value = ParseMultiple(parent, expectedTypes: Spec.PrimaryExprs);
                    break;
                }

                case OpenBracket: {
                    value = new ListInitializerExpression(parent);
                    break;
                }

                case OpenBrace: {
                    value = new HashCollectionExpression(parent);
                    break;
                }

                default: {
                    parent.Move();
                    if (Spec.Literals.Contains(parent.Token.Type)) {
                        // TODO add pre-concatenation of literals
                        value = new ConstantExpression(parent);
                    }
                    else {
                        parent.Unit.ReportError(Spec.ERR_PrimaryExpected, parent.Token);
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
        ///             (( primary {'|>' primary }) # pipeline
        ///              | { member | call_expr | index_expr })
        ///             ['++' | '--']
        ///     </c>
        /// </summary>
        internal static Expression ParseExtended(
            SyntaxTreeNode parent,
            bool           allowGenerator = false
        ) {
            Expression value = ParsePrimary(parent);
            // pipeline cannot be combined with other trailers
            if (parent.MaybeEat(RightPipeline)) {
                do {
                    value = new FunctionCallExpression(
                        parent,
                        ParsePrimary(parent),
                        new CallArgument(parent, value)
                    );
                } while (parent.MaybeEat(RightPipeline));

                return value;
            }

            while (true) {
                if (parent.PeekIs(Dot)) {
                    value = new MemberAccessExpression(parent, value);
                }
                else if (parent.PeekIs(OpenParenthesis)) {
                    value = new FunctionCallExpression(parent, value, allowGenerator);
                }
                else if (parent.PeekIs(OpenBracket)) {
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
        ///             (UNARY_LEFT unary_left) | trailer
        ///     </c>
        /// </summary>
        internal static Expression ParseUnaryLeftExpr(SyntaxTreeNode parent) {
            if (parent.MaybeEat(Spec.UnaryLeftOperators)) {
                var op = (OperatorToken) parent.Token;
                op.Properties.InputSide = InputSide.Left;
                return new UnaryOperationExpression(parent, op, ParseUnaryLeftExpr(parent));
            }

            return ParseExtended(parent, true);
        }

        /// <summary>
        ///     <c>
        ///         priority_expr:
        ///             factor OPERATOR priority_expr
        ///     </c>
        /// </summary>
        internal static Expression ParseOperation(SyntaxTreeNode parent, int precedence = 0) {
            Expression expr = ParseUnaryLeftExpr(parent);
            while (parent.Peek is OperatorToken op && op.Properties.Precedence >= precedence) {
                parent.Move();
                expr = new BinaryOperationExpression(
                    expr,
                    op,
                    ParseOperation(parent, op.Properties.Precedence + 1)
                );
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         test:
        ///             priority_expr [cond_expr]
        ///             | lambda_def
        ///     </c>
        /// </summary>
        internal static Expression ParseTestExpr(SyntaxTreeNode parent) {
            Expression expr = ParseOperation(parent);
            if (parent.PeekIs(KeywordIf, KeywordUnless)
                && parent.Token.Type != Newline) {
                expr = new ConditionalExpression(parent, expr);
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         expr:
        ///             test | (['let'] assignable [':' type] ['=' assign_value])
        ///         assign_value:
        ///             yield_expr | test_list
        ///     </c>
        /// </summary>
        internal static Expression ParseExpression(SyntaxTreeNode parent) {
            bool isImmutable = parent.MaybeEat(KeywordLet);

            Expression expr = ParseMultiple(parent, ParseTestExpr);

            if (expr is ErrorExpression
                || expr is UnaryOperationExpression
                || !(isImmutable
                     || Spec.AssignableExprs.Contains(expr.GetType())
                     && parent.PeekIs(Colon, OpAssign))) {
                return expr;
            }

            if (expr is BinaryOperationExpression bin && bin.Operator.Is(OpAssign)) {
                return new VariableDefinitionExpression(bin.Left, null, bin.Right) {
                    IsImmutable = isImmutable
                };
            }

            if (!Spec.AssignableExprs.Contains(expr.GetType())) {
                parent.Unit.ReportError(Spec.ERR_InvalidAssignmentTarget, expr);
            }

            TypeName   type  = null;
            Expression value = null;
            if (parent.MaybeEat(Colon)) {
                type = TypeName.ParseTypeName(parent);
            }

            if (parent.MaybeEat(OpAssign)) {
                value = ParseMultiple(parent, expectedTypes: Spec.TestExprs);
            }

            return new VariableDefinitionExpression(expr, type, value) {
                IsImmutable = isImmutable
            };
        }

        /// <summary>
        ///     <c>
        ///         ['('] expr {',' expr} [')']
        ///     </c>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal static Expression ParseMultiple(
            SyntaxTreeNode                   parent,
            Func<SyntaxTreeNode, Expression> parserFunc = null,
            params Type[]                    expectedTypes
        ) {
            parserFunc   ??= ParseExpression;
            bool  parens =   parent.MaybeEat(OpenParenthesis);
            Token start  =   parent.Token;
            // empty tuple
            if (parent.MaybeEat(CloseParenthesis)) {
                return new TupleExpression(parent, start, parent.Token);
            }

            var list = new NodeList<Expression>(parent) {
                parserFunc(parent)
            };

            // tuple
            if (parent.MaybeEat(Comma)) {
                bool trailingComma;
                do {
                    list.Add(parserFunc(parent));
                    trailingComma = parent.MaybeEat(Comma);
                } while (trailingComma);
            }
            // generator | comprehension
            else if (parent.PeekIs(KeywordFor)) {
                list[0] = new ForComprehension(parent, list[0]);
                if (parens) {
                    list[0] = new GeneratorExpression((ForComprehension) list[0]);
                }
            }
            // parenthesized
            else {
                if (parens
                    && !(list[0] is ParenthesizedExpression)
                    && !(list[0] is TupleExpression)) {
                    list[0] = new ParenthesizedExpression(list[0]);
                }
            }

            if (parens) {
                parent.Eat(CloseParenthesis);
            }

            if (expectedTypes.Length > 0) {
                for (var i = 0; i < list.Count; i++) {
                    Type itemType = list[i].GetType();
                    if (!expectedTypes.Contains(itemType)) {
                        parent.Unit.ReportError(
                            "Expected "
                            + Utilities.GetExprFriendlyName(expectedTypes[0])
                            + ", got "
                            + Utilities.GetExprFriendlyName(itemType),
                            list[i]
                        );
                    }
                }
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
    }
}