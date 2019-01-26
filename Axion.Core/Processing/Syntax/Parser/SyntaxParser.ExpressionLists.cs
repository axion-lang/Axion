using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        /// <c>
        ///     expression_list:
        ///         expression (',' expression)* [',']
        /// </c>
        /// </summary>
        private List<Expression> ParseExpressionList(out bool trailingComma, bool old = false) {
            var list = new List<Expression>();
            trailingComma = false;
            while (!stream.PeekIs(Spec.NeverTestTypes)) {
                list.Add(ParseTestExpr(old));
                trailingComma = stream.MaybeEat(Comma);
                if (!trailingComma) {
                    break;
                }
            }
            return list;
        }

        private Expression ParseTestListAsExpr() {
            if (stream.PeekIs(Spec.NeverTestTypes)) {
                return ParseTestListAsExprError();
            }
            Expression expr = ParseTestExpr();
            if (stream.MaybeEat(Comma)) {
                return MakeTupleOrExpr(ParseExpressionList(out bool trailingComma), trailingComma);
            }
            return expr;
        }

        private Expression ParseTestListStarExpr() {
            if (stream.MaybeEat(OpMultiply)) {
                var expr = new StarredExpression(tokenStart, ParseTestExpr());
                if (stream.MaybeEat(Comma)) {
                    return ParseTestListStarExpr(expr);
                }
                return expr;
            }
            if (!stream.PeekIs(Spec.NeverTestTypes)) {
                Expression expr = ParseTestExpr();
                if (stream.MaybeEat(Comma)) {
                    return ParseTestListStarExpr(expr);
                }
                return expr;
            }
            return ParseTestListAsExprError();
        }

        private Expression ParseTestListStarExpr(Expression expr) {
            var list = new List<Expression> { expr };

            var trailingComma = true;
            do {
                if (stream.MaybeEat(OpMultiply)) {
                    list.Add(new StarredExpression(tokenStart, ParseTestExpr()));
                }
                else if (stream.PeekIs(Spec.NeverTestTypes)) {
                    break;
                }
                else {
                    list.Add(ParseTestExpr());
                }
                trailingComma = stream.MaybeEat(Comma);
            } while (trailingComma);
            return MakeTupleOrExpr(list, trailingComma);
        }

        private Expression ParseTestListAsExprError() {
            if (stream.MaybeEat(Indent)) {
                // the error is on the next token which has
                // a useful location, unlike the indent - note we don't have an
                // indent if we're at an EOS.
                Blame(BlameType.UnexpectedIndentation, stream.NextToken());
            }
            else {
                BlameInvalidSyntax(Indent, stream.Peek);
            }

            return Error();
        }

        /// <summary>
        /// <c>
        ///     expr_list:
        ///         (expr|star_expr) (',' (expr|star_expr))* [',']
        /// </c>
        /// </summary>
        private List<Expression> ParseExprList() {
            var expressions = new List<Expression>();
            do {
                expressions.Add(ParseExpr());
            } while (
                stream.MaybeEat(Comma) &&
                !stream.PeekIs(Spec.NeverTestTypes)
            );
            return expressions;
        }

        private List<Expression> ParseTestList() {
            return ParseExpressionList(out bool _);
        }

        /// <summary>
        /// <c>
        ///     target_list:
        ///         target ("," target)* [","] 
        /// </c>
        /// </summary>
        private List<Expression> ParseTargetListExpr(out bool trailingComma) {
            var list = new List<Expression>();
            do {
                list.Add(ParseTargetExpr());
                trailingComma = stream.MaybeEat(Comma);
            } while (
                trailingComma
             && !stream.PeekIs(Spec.NeverTestTypes)
            );
            return list;
        }

        /// <summary>
        /// <c>
        ///     target:
        ///         ID                  |
        ///         "(" target_list ")" |
        ///         "[" target_list "]" |
        ///         attribute_ref       |
        ///         subscription        |
        ///         slicing
        /// </c>
        /// </summary>
        private Expression ParseTargetExpr() {
            Token startToken = stream.Peek;
            if (stream.MaybeEat(LeftParenthesis, LeftBracket)) {
                // parenthesis_form | generator_expression
                Expression result = MakeTupleOrExpr(ParseTargetListExpr(out bool trailingComma), trailingComma);
                stream.Eat(((SymbolToken) startToken).GetMatchingBrace());
                return result;
            }
            return ParseTrailingExpr(ParsePrimaryExpr(), false);
        }

        /// <summary>
        ///     If <param name="trailingComma" />, creates a tuple,
        ///     otherwise, returns first expression from list.
        /// </summary>
        private static Expression MakeTupleOrExpr(List<Expression> expressions, bool trailingComma, bool expandable = false) {
            if (expressions.Count == 1 && !trailingComma) {
                return expressions[0];
            }
            return new TupleExpression(expandable && !trailingComma, expressions.ToArray());
        }
    }
}