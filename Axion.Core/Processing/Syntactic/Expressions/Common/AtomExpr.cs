using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common {
    /// <summary>
    ///     <c>
    ///         atom
    ///             : name
    ///             | await-expr
    ///             | parenthesis-expr
    ///             | CONSTANT;
    ///     </c>
    /// </summary>
    public class AtomExpr : PostfixExpr {
        protected AtomExpr() { }

        protected AtomExpr(Expr parent) : base(parent) { }

        internal new static AtomExpr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            if (s.PeekIs(Identifier) && !parent.Source.IsCustomKeyword(s.Peek)) {
                return new NameExpr(parent).Parse(true);
            }
            if (s.PeekIs(KeywordAwait)) {
                return new AwaitExpr(parent).Parse();
            }
            if (s.PeekIs(KeywordYield)) {
                return new YieldExpr(parent).Parse();
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

                return Multiple<InfixExpr>.ParseGenerally(parent);
            }
            if (Spec.Constants.Contains(s.Peek.Type)) {
                return new ConstantExpr(parent).Parse();
            }

            MacroApplicationExpr macro = new MacroApplicationExpr(parent).Parse();
            if (macro.Macro != null) {
                return macro;
            }

            return new UnknownExpr(parent).Parse();
        }
    }
}