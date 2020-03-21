using System;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic {
    /// <summary>
    ///     <c>
    ///         ('(' inner ')') | inner;
    ///         inner:
    ///             %expr {',' %expr}
    ///     </c>
    /// </summary>
    public class Multiple<T> : AtomExpr where T : Expr {
        protected Multiple(Expr parent) : base(parent) { }

        /// <summary>
        ///     Parses multiple of ANY expr and ensures that it's similar to type of T.
        /// </summary>
        internal static Multiple<Expr> ParseGenerally(Expr parent) {
            // TODO: add check for <T> compliance.
            return Multiple<Expr>.Parse(parent);
        }

        /// <summary>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal new static Multiple<T> Parse(Expr parent) {
            TokenStream      s          = parent.Source.TokenStream;
            Func<Expr, Expr> parserFunc = Auxiliary.GetParsingFunction<T>();
            bool             parens     = s.MaybeEat(OpenParenthesis);
            var              list       = new NodeList<Expr>(parent) { parserFunc(parent) };

            // tuple
            if (s.MaybeEat(Comma)) {
                do {
                    list.Add(parserFunc(parent));
                } while (s.MaybeEat(Comma));
            }
            
            if (parens) {
                s.Eat(CloseParenthesis);
                if (list.Count == 1) {
                    return new ParenthesizedExpr<T>(list[0]);
                }
            }

            return new TupleExpr<T>(list.Parent, list);
        }
    }
}