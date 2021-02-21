using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common {
    /// <summary>
    ///     "Atom" expression has no detachable parts.
    ///     <br/>
    ///     It is meant to be embedded in other expressions.
    ///     <br/>
    ///     Exceptions, like 'await' or 'yield' can appear
    ///     on the top level of <see cref="AnyExpr"/> if needed,
    ///     but they're still embeddable ones.
    ///     <c>
    ///         atom
    ///             : name
    ///             | await-expr
    ///             | yield-expr
    ///             | anon-fn-expr
    ///             | code-quote-expr
    ///             | tuple-expr
    ///             | parenthesis-expr
    ///             | const-expr
    ///             | atom-macro-expr
    ///             | unknown-expr;
    ///     </c>
    /// </summary>
    public class AtomExpr : PostfixExpr {
        protected AtomExpr(Node parent) : base(parent) { }

        internal new static AtomExpr Parse(Node parent) {
            var s = parent.Unit.TokenStream;

            if (s.PeekIs(Identifier)
             && !parent.Unit.Module.CustomKeywords.Contains(s.Peek.Content)) {
                return new NameExpr(parent).Parse(true);
            }

            if (s.PeekIs(KeywordFn)) {
                return new FunctionDef(parent).Parse(true);
            }

            if (s.PeekIs(DoubleOpenBrace)) {
                return new CodeQuoteExpr(parent).Parse();
            }

            if (s.PeekIs(OpenParenthesis)) {
                // empty tuple
                if (s.PeekByIs(2, CloseParenthesis)) {
                    return new TupleExpr(parent).ParseEmpty();
                }

                return Multiple.ParsePermissively<InfixExpr>(parent);
            }

            if (Spec.Constants.Contains(s.Peek.Type)) {
                return new ConstantExpr(parent).Parse();
            }

            var macro = new MacroMatchExpr(parent).Parse();
            if (macro.Macro != null) {
                return macro;
            }

            return new UnknownExpr(parent).Parse();
        }
    }
}
