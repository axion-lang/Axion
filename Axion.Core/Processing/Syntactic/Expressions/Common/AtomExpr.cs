using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common;

/// <summary>
///     "Atom" expression has no detachable parts.
///     It is meant to be embedded in other expressions.
///     <br />
///     <code>
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
///     </code>
/// </summary>
public class AtomExpr : PostfixExpr {
    protected AtomExpr(Node? parent) : base(parent) { }

    internal new static AtomExpr Parse(Node parent) {
        var s = parent.Unit.TokenStream;

        if (s.PeekIs(Identifier)
         && (parent.Unit.Module == null
          || !parent.Unit.Module.CustomKeywords.Contains(s.Peek.Content))) {
            return new NameExpr(parent).Parse(true);
        }

        // lambda
        if (s.PeekIs(KeywordFn)) {
            return new FunctionDef(parent).Parse(true);
        }

        if (s.PeekIs(DoubleOpenBrace)) {
            var quote = new CodeQuoteExpr(parent).Parse();
            if (parent.GetParent<MacroDef>() == null) {
                LanguageReport.To(BlameType.CodeQuoteOutsideMacroDef, quote);
            }
            return quote;
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

        var match = new MacroMatchExpr(parent).Parse();
        if (match.Macro != null) {
            return match;
        }

        return new UnknownExpr(parent).Parse();
    }
}
