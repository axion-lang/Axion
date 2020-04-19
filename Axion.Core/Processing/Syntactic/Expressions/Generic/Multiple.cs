using System;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic {
    /// <summary>
    ///     <c>
    ///         ('(' inner ')') | inner;
    ///         inner:
    ///             %expr {',' %expr}
    ///     </c>
    ///     Provides functions for comma-separated expression lists
    ///     parsing with automatic error reporting. 
    /// </summary>
    public class Multiple<T> : AtomExpr
        where T : Expr {
        protected Multiple(Node parent) : base(parent) { }

        /// <summary>
        ///     Parses multiple of <see cref="AnyExpr"/> and
        ///     reports errors if any of them is not compliant to T.
        /// </summary>
        internal static AtomExpr ParseGenerally(Expr parent) {
            AtomExpr e = Multiple<Expr>.Parse(parent);
            if (e is TupleExpr tpl) {
                foreach (Expr expr in tpl.Expressions) {
                    if (!(expr is T)) {
                        LangException.ReportUnexpectedType(typeof(T), expr);
                    }
                }
            }
            else {
                if (!(e is T)) {
                    LangException.ReportUnexpectedType(typeof(T), e);
                }
            }
            return e;
        }

        /// <summary>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal new static AtomExpr Parse(Expr parent) {
            TokenStream      s          = parent.Source.TokenStream;
            Func<Expr, Expr> parserFunc = Auxiliary.GetParsingFunction<T>();
            bool             parens     = s.MaybeEat(OpenParenthesis);
            var list = new NodeList<Expr>(parent) {
                parserFunc(parent)
            };
            // tuple
            while (s.MaybeEat(Comma)) {
                list.Add(parserFunc(parent));
            }
            if (parens) {
                s.Eat(CloseParenthesis);
            }
            if (list.Count > 1) {
                return new TupleExpr(list.Parent!) {
                    Expressions = list
                };
            }
            return new ParenthesizedExpr(list[0]);
        }
    }
}
