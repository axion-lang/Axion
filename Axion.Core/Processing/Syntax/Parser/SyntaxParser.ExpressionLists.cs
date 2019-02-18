using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.Multiple;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         expr_list ::=
        ///             expr {',' expr} [',']
        ///     </c>
        /// </summary>
        private List<Expression> ParseExpressionList(out bool trailingComma) {
            var list = new List<Expression>();
            trailingComma = false;
            do {
                list.Add(ParseTestExpr());
                trailingComma = stream.MaybeEat(Comma);
            } while (trailingComma && !stream.PeekIs(Spec.NeverTestTypes));
            return list;
        }

        /// <summary>
        ///     <c>
        ///         test_list ::=
        ///             '(' ')' |
        ///             ['('] expr_list [')']
        ///     </c>
        /// </summary>
        private Expression ParseTestList(bool allowEmpty = false) {
            bool       parens = stream.MaybeEat(LeftParenthesis);
            Expression expr   = null;
            if (stream.PeekIs(Spec.NeverTestTypes)) {
                if (!allowEmpty) {
                    ReportError("Invalid expression.", stream.Peek);
                    expr = Error();
                    stream.NextToken();
                }
            }
            else {
                expr = ParseTestExpr();
                if (stream.MaybeEat(Comma)) {
                    List<Expression> list = ParseExpressionList(out bool trailingComma);
                    list.Insert(0, expr);
                    expr = MakeTupleOrExpr(list, trailingComma);
                }
            }
            if (parens) {
                stream.Eat(RightParenthesis);
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
        private List<Expression> ParseTargetList(out bool trailingComma) {
            var list = new List<Expression>();

            do {
                if (stream.MaybeEat(LeftParenthesis, LeftBracket)) {
                    var brace = (SymbolToken) stream.Token;

                    // parenthesis_form | generator_expr
                    list.Add(MakeTupleOrExpr(ParseTargetList(out trailingComma), trailingComma));
                    stream.Eat(brace.GetMatchingBrace());
                }
                else {
                    list.Add(ParseTrailingExpr(ParsePrimaryExpr(), false));
                }

                trailingComma = stream.MaybeEat(Comma);
            } while (trailingComma && !stream.PeekIs(Spec.NeverTestTypes));

            return list;
        }

        /// <summary>
        ///     If <paramref name="trailingComma" />, creates a tuple,
        ///     otherwise, returns first expr from list.
        /// </summary>
        private static Expression MakeTupleOrExpr(
            List<Expression> expressions,
            bool             trailingComma,
            bool             expandable = false
        ) {
            if (!trailingComma && expressions.Count == 1) {
                return expressions[0];
            }
            return new TupleExpression(expandable && !trailingComma, expressions.ToArray());
        }
    }
}