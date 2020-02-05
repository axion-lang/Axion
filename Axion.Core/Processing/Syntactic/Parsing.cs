using System;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic {
    public static class Parsing {
        /// <summary>
        ///     <c>
        ///         ['('] %expr {',' %expr} [')'];
        ///     </c>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal static Expr MultipleExprs(
            Expr             parent,
            Func<Expr, Expr> parserFunc = null,
            params Type[]    expectedTypes
        ) {
            TokenStream s = parent.Source.TokenStream;
            parserFunc ??= AnyExpr.Parse;
            bool parens = s.MaybeEat(OpenParenthesis);
            var list = new NodeList<Expr>(parent) {
                parserFunc(parent)
            };

            // tuple
            if (s.MaybeEat(Comma)) {
                do {
                    list.Add(parserFunc(parent));
                } while (s.MaybeEat(Comma));
            }
            // generator | comprehension
            // TODO HERE 'for' can be after 'newline', but if it's inside (), {} or []
            else if (parens && list[0] is ForComprehension fcomp) {
                s.Eat(CloseParenthesis);
                fcomp.IsGenerator = true;
                return fcomp;
            }

            if (parens) {
                s.Eat(CloseParenthesis);
                if (list.Count == 1) {
                    return new ParenthesizedExpr(list[0]);
                }
            }

            return MaybeTuple(list);
        }

        internal static Expr MaybeTuple(NodeList<Expr> expressions) {
            if (expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpr(expressions.Parent, expressions);
        }
    }
}