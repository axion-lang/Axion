using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions;
using Axion.Core.Processing.Syntax.Tree.Expressions.Multiple;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         primary ::=
        ///             atom | trailer
        ///         atom ::=
        ///             ID
        ///             | LITERAL
        ///             | parenthesis_expr
        ///             | list_display
        ///             | map_or_set_display
        ///     </c>
        /// </summary>
        private Expression ParsePrimaryExpr() {
            Token token = stream.Peek;
            switch (token.Type) {
                case TokenType.Identifier: {
                    stream.NextToken();
                    return new NameExpression(token);
                }
                case TokenType.LeftParenthesis: {
                    return ParsePrimaryInParenthesis();
                }
                case TokenType.LeftBracket: {
                    return ParsePrimaryInBrackets();
                }
                case TokenType.LeftBrace: {
                    return ParseMapOrSetDisplay();
                }
                default: {
                    if (Spec.ConstantValueTypes.Contains(token.Type)) {
                        stream.NextToken();
                        // TODO add pre-concatenation of literals
                        return new ConstantExpression(token);
                    }
                    ReportError(Spec.ERR_PrimaryExpected, token);
                    return Error();
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         parenthesis_expr
        ///         | yield_expr
        ///         | {expr (',' expr} [','] )
        ///         | generator_expr
        ///     </c>
        /// </summary>
        private Expression ParsePrimaryInParenthesis() {
            Position start = StartExprOrStmt(TokenType.LeftParenthesis).Span.StartPosition;

            Expression result;
            if (stream.PeekIs(TokenType.RightParenthesis)) {
                result = new TupleExpression(false, new Expression[0]);
            }
            // yield_expr
            else if (stream.PeekIs(TokenType.KeywordYield)) {
                result = ParseYield();
            }
            else {
                Expression expr = ParseExpression();
                // tuple
                if (stream.MaybeEat(TokenType.Comma)) {
                    // '(' expr ',' ...
                    result = ParseTestList();
                }
                // generator_expr
                else if (stream.PeekIs(TokenType.KeywordFor)) {
                    // '(' expr 'for' ...
                    result = ParseGeneratorExpr(expr);
                }
                // parenthesis_expr
                else {
                    // '(' expr ')'
                    result = expr is ParenthesisExpression || expr is TupleExpression
                                 ? expr
                                 : new ParenthesisExpression(expr);
                }
            }
            stream.Eat(TokenType.RightParenthesis);
            result.MarkPosition(start, tokenEnd);
            return result;
        }

        /// <summary>
        ///     <c>
        ///         generator_expr ::=
        ///             '(' expr comp_for ')'
        ///     </c>
        ///     "for" has NOT been stream.Eaten before entering this method
        /// </summary>
        private Expression ParseGeneratorExpr(Expression expr) {
            var comprehensions = new List<ComprehensionIterator> { ParseComprehensionFor() };
            while (true) {
                if (stream.PeekIs(TokenType.KeywordFor)) {
                    comprehensions.Add(ParseComprehensionFor());
                }
                else if (stream.PeekIs(TokenType.KeywordIf)) {
                    comprehensions.Add(ParseComprehensionIf());
                }
                else {
                    break;
                }
            }
            return new GeneratorExpression(expr, comprehensions.ToArray());
        }

        /// <summary>
        ///     <c>
        ///         list_display | subscription
        ///         list_display ::=
        ///             '[' expr ( comprehension_iterator | {',' expr} [','] ) ']'
        ///     </c>
        /// </summary>
        private Expression ParsePrimaryInBrackets() {
            Position start = StartExprOrStmt(TokenType.LeftBracket).Span.StartPosition;

            var expressions = new List<Expression>();
            if (!stream.MaybeEat(TokenType.RightBracket)) {
                expressions.Add(ParseTestExpr());
                if (stream.MaybeEat(TokenType.Comma)) {
                    expressions.AddRange(ParseTestList(out bool _));
                }
                else if (stream.PeekIs(TokenType.KeywordFor)) {
                    ComprehensionIterator[] iterators = ParseComprehensionIterators();
                    stream.Eat(TokenType.RightBracket);
                    return new ListComprehension(expressions[0], iterators, start, tokenEnd);
                }
                stream.Eat(TokenType.RightBracket);
            }
            return new ListExpression((start, tokenEnd), expressions.ToArray());
        }

        /// <summary>
        ///     <c>
        ///         map_or_set_display ::=
        ///             '{' [map_or_set_initializer] '}'
        ///         map_or_set_initializer ::=
        ///         (
        ///             (test ':' test
        ///                 (comprehension_for
        ///                 | {',' test ':' test} [','])
        ///             )
        ///             | (test (comprehension_for | {',' test} [',']))
        ///         )
        ///     </c>
        /// </summary>
        private Expression ParseMapOrSetDisplay() {
            Position start = StartExprOrStmt(TokenType.LeftBrace).Span.StartPosition;

            List<SliceExpression> mapMembers = null;
            List<Expression>      setMembers = null;

            while (!stream.MaybeEat(TokenType.RightBrace)) {
                var        first     = false;
                Expression itemPart1 = ParseTestExpr();

                // map item (expr : expr)
                if (stream.MaybeEat(TokenType.Colon)) {
                    if (setMembers != null) {
                        ReportError("Single expression expected", stream.Token);
                    }
                    else if (mapMembers == null) {
                        mapMembers = new List<SliceExpression>();
                        first      = true;
                    }
                    Expression itemPart2 = ParseTestExpr();
                    // map generator: { key: value for (key, value) in iterable }
                    if (stream.PeekIs(TokenType.KeywordFor)) {
                        if (!first) {
                            ReportError(
                                "Generator can only be used as single map item",
                                stream.Token
                            );
                        }
                        return FinishMapComprehension(itemPart1, itemPart2, start);
                    }
                    mapMembers?.Add(new SliceExpression(itemPart1, itemPart2, null));
                }
                // set item (expr)
                else {
                    if (mapMembers != null) {
                        ReportError("'Key : Value' expression expected", stream.Token);
                    }
                    else if (setMembers == null) {
                        setMembers = new List<Expression>();
                        first      = true;
                    }
                    // set generator: { x * 2 for x in { 1, 2, 3 } }
                    if (stream.PeekIs(TokenType.KeywordFor)) {
                        if (!first) {
                            ReportError(
                                "Generator can only be used as single set item",
                                stream.Token
                            );
                        }
                        return FinishSetComprehension(itemPart1, start);
                    }
                    setMembers?.Add(itemPart1);
                }

                if (!stream.MaybeEat(TokenType.Comma)) {
                    stream.Eat(TokenType.RightBrace);
                    break;
                }
            }

            if (mapMembers == null && setMembers != null) {
                return new SetExpression(setMembers.ToArray(), start, tokenEnd);
            }
            return new MapExpression(
                mapMembers?.ToArray() ?? new SliceExpression[0],
                start,
                tokenEnd
            );
        }
    }
}