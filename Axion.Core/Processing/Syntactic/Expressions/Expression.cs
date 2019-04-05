using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public abstract class Expression : SyntaxTreeNode {
        internal virtual string CannotDeleteReason => null;
        internal virtual string CannotAssignReason => null;

        /// <summary>
        ///     <c>
        ///         primary ::=
        ///             ID
        ///             | CONSTANT
        ///             | parenthesis_expr
        ///             | list_expr
        ///             | hash_collection
        ///             | trailer
        ///     </c>
        /// </summary>
        internal static Expression ParsePrimary(SyntaxTreeNode parent) {
            Token token = parent.Peek;
            switch (token.Type) {
                case TokenType.Identifier: {
                    return new NameExpression(parent);
                }

                case TokenType.OpenParenthesis: {
                    return ParenthesisExpression.Parse(parent);
                }

                case TokenType.OpenBracket: {
                    return new ListExpression(parent);
                }

                case TokenType.OpenBrace: {
                    return new HashCollectionExpression(parent);
                }

                default: {
                    if (Spec.Literals.Contains(token.Type)) {
                        parent.NextToken();
                        // TODO add pre-concatenation of literals
                        return new ConstantExpression(token);
                    }

                    parent.NextToken();
                    parent.Unit.ReportError(Spec.ERR_PrimaryExpected, token);
                    return new ErrorExpression(parent.Token);
                }
            }
        }

        /// <summary>
        ///     If <paramref name="trailingComma" />, creates a tuple,
        ///     otherwise, returns first expr from list.
        /// </summary>
        internal static Expression MaybeTuple(
            NodeList<Expression> expressions,
            bool                 trailingComma
        ) {
            if (!trailingComma && expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpression(!trailingComma, expressions);
        }

        /// <summary>
        ///     <c>
        ///         test_list ::=
        ///             ['('] [expr_list] [')']
        ///     </c>
        /// </summary>
        internal static Expression SingleOrTuple(
            SyntaxTreeNode parent,
            bool           allowEmptyTuple = false
        ) {
            Expression expr = null;
            if (parent.PeekIs(Spec.NeverTestTypes)) {
                if (!allowEmptyTuple) {
                    parent.Unit.ReportError("Invalid expression.", parent.Peek);
                    expr = new ErrorExpression(parent.Token);
                    parent.NextToken();
                }
            }
            else {
                expr = ParseTestExpr(parent);
                if (parent.MaybeEat(TokenType.Comma)) {
                    var list = new TestList(parent, out bool trailingComma);
                    list.Insert(0, expr);
                    expr = MaybeTuple(list.Expressions, trailingComma);
                }
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         target ::=
        ///             ID
        ///             | '(' target_list ')'
        ///             | '[' target_list ']'
        ///             | primary [trailer]
        ///         target_list ::=
        ///             target {',' target} [',']
        ///     </c>
        /// </summary>
        internal static NodeList<Expression> TargetList(
            SyntaxTreeNode parent,
            out bool       trailingComma
        ) {
            var list = new NodeList<Expression>(parent);

            do {
                if (parent.MaybeEat(TokenType.OpenParenthesis, TokenType.OpenBracket)) {
                    var brace = (MarkToken) parent.Token;

                    // parenthesis_form | generator_expr
                    list.Add(MaybeTuple(TargetList(parent, out trailingComma), trailingComma));
                    parent.Eat(brace.Type.GetMatchingBracket());
                }
                else {
                    list.Add(ParseTrailingExpr(parent, false));
                }

                trailingComma = parent.MaybeEat(TokenType.Comma);
            } while (trailingComma && !parent.PeekIs(Spec.NeverTestTypes));

            return list;
        }

        /// <summary>
        ///     <c>
        ///         trailing ::=
        ///             | '[' subscription_list ']'
        ///             | '++' | '--'
        ///             | call
        ///         call ::=
        ///             '(' [args_list | comprehension] ')'
        ///         member ::=
        ///             '.' ID
        ///     </c>
        /// </summary>
        internal static Expression ParseTrailingExpr(
            SyntaxTreeNode parent,
            bool           allowGeneratorExpression
        ) {
            Expression result = ParsePrimary(parent);
            while (true) {
                if (parent.MaybeEat(TokenType.Dot)) {
                    result = new MemberExpression(result, new NameExpression(parent));
                }

                if (parent.MaybeEat(TokenType.RightPipeline)) {
                    result = new CallExpression(
                        parent,
                        ParseTestExpr(parent),
                        new Arg(result)
                    );
                }
                else if (parent.Peek.Type == TokenType.OpenBracket) {
                    result = new IndexExpression(result, IndexExpression.Parse(parent));
                }
                else if (parent.MaybeEat(TokenType.OpIncrement, TokenType.OpDecrement)) {
                    var op = (OperatorToken) parent.Token;
                    op.Properties.InputSide = InputSide.Right;
                    result                  = new UnaryExpression(op, result);
                }
                else if (parent.PeekIs(TokenType.OpenParenthesis)) {
                    result = new CallExpression(parent, result, allowGeneratorExpression);
                }
                else {
                    return result;
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         unary_left ::=
        ///             (UNARY_LEFT unary_left) | trailer
        ///     </c>
        /// </summary>
        internal static Expression ParseUnaryLeftExpr(SyntaxTreeNode parent) {
            if (parent.MaybeEat(Spec.UnaryLeftOperators)) {
                var op = (OperatorToken) parent.Token;
                op.Properties.InputSide = InputSide.Left;
                return new UnaryExpression(op, ParseUnaryLeftExpr(parent));
            }

            return ParseTrailingExpr(parent, true);
        }

        /// <summary>
        ///     <c>
        ///         priority_expr ::=
        ///             factor OPERATOR priority_expr
        ///     </c>
        /// </summary>
        internal static Expression ParseOperation(SyntaxTreeNode parent, int precedence = 0) {
            Expression expr = ParseUnaryLeftExpr(parent);
            while (parent.Peek is OperatorToken op
                   && op.Properties.Precedence >= precedence) {
                parent.NextToken();
                expr = new BinaryExpression(
                    expr,
                    op,
                    ParseOperation(parent, op.Properties.Precedence + 1)
                );
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         test ::=
        ///             priority_expr [cond_expr]
        ///             | lambda_def
        ///     </c>
        /// </summary>
        internal static Expression ParseTestExpr(SyntaxTreeNode parent) {
            Expression expr = ParseOperation(parent);
            if (parent.PeekIs(TokenType.KeywordIf, TokenType.KeywordUnless)
                && parent.Token.Type != TokenType.Newline) {
                expr = new ConditionalExpression(parent, expr);
            }

            return expr;
        }

        /// <summary>
        ///     <c>
        ///         expr ::=
        ///             ['let'] test_list [':' type] '=' assign_value
        ///         assign_value ::=
        ///             yield_expr | test_list
        ///     </c>
        /// </summary>
        internal static Expression ParseExpression(SyntaxTreeNode parent) {
            bool isImmutable = parent.MaybeEat(TokenType.KeywordLet);

            Expression expr = SingleOrTuple(parent);
            if (expr is ErrorExpression
                || expr is UnaryExpression) {
                return expr;
            }

            // 'let' expr: type ['=' expr]
            if (isImmutable) {
                if (expr.CannotAssignReason != null) {
                    parent.Unit.ReportError(expr.CannotAssignReason, expr);
                }

                TypeName   type  = null;
                Expression value = null;
                if (parent.MaybeEat(TokenType.Colon)) {
                    type = TypeName.Parse(parent);
                }

                if (parent.MaybeEat(TokenType.OpAssign)) {
                    value = ParseValue(parent);
                }

                return new VarDefinitionExpression(expr, type, value) {
                    IsImmutable = true
                };
            }

            // expr: type ['=' expr]
            if (parent.MaybeEat(TokenType.Colon)) {
                TypeName   type  = TypeName.Parse(parent);
                Expression value = null;
                if (parent.MaybeEat(TokenType.OpAssign)) {
                    value = ParseValue(parent);
                }

                if (expr.CannotAssignReason != null) {
                    parent.Unit.ReportError(expr.CannotAssignReason, expr);
                }

                return new VarDefinitionExpression(expr, type, value);
            }

            return expr;
        }

        private static Expression ParseValue(SyntaxTreeNode parent) {
            return parent.PeekIs(TokenType.KeywordYield)
                ? new YieldExpression(parent)
                : SingleOrTuple(parent);
        }
    }
}