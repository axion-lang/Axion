using System.Collections.Generic;
using System.Linq;
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
            while (!Spec.NeverTestTypes.Contains(stream.Peek.Type)) {
                list.Add(ParseTestExpr(old));
                trailingComma = stream.MaybeEat(Comma);
                if (!trailingComma) {
                    break;
                }
            }
            return list;
        }

        private Expression ParseTestListAsExpr() {
            if (Spec.NeverTestTypes.Contains(stream.Peek.Type)) {
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
            if (!Spec.NeverTestTypes.Contains(stream.Peek.Type)) {
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
                else if (Spec.NeverTestTypes.Contains(stream.Peek.Type)) {
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
                // indent if we're at an EOF.
                stream.NextToken();
                Blame(BlameType.UnexpectedIndentation, stream.Token);
            }
            else {
                BlameInvalidSyntax(Indent, stream.Peek);
            }

            return Error();
        }

        /// <summary>
        ///     expr_list:
        ///     (expr|star_expr) (',' (expr|star_expr))* [',']
        /// </summary>
        private List<Expression> ParseExprList() {
            var expressions = new List<Expression>();
            do {
                expressions.Add(ParseExpr());
            } while (
                stream.MaybeEat(Comma) &&
                !Spec.NeverTestTypes.Contains(stream.Peek.Type)
            );
            return expressions;
        }

        private List<Expression> ParseTestList() {
            return ParseExpressionList(out bool _);
        }

        // target_list: target ("," target)* [","] 
        private List<Expression> ParseTargetListExpr(out bool trailingComma) {
            var list = new List<Expression>();
            do {
                list.Add(ParseTargetExpr());
                trailingComma = stream.MaybeEat(Comma);
            } while (
                trailingComma
             && !Spec.NeverTestTypes.Contains(stream.Peek.Type)
            );
            return list;
        }

        /// <summary>
        ///     target:
        ///     identifier          |
        ///     "(" target_list ")" |
        ///     "[" target_list "]" |
        ///     attribute_ref       |
        ///     subscription        |
        ///     slicing
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
            return new TupleExpression(expandable && !trailingComma, expressions);
        }
    }
}