using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common;

/// <summary>
///     "Any" is a top level expression that
///     don't appear inside other expressions.
///     <br />
///     Any expression embeds <see cref="InfixExpr" /> in itself,
///     but not every <see cref="InfixExpr" /> is <see cref="AnyExpr" />.
///     <br />
///     This class is too general and it has no strict hierarchy,
///     that's why <see cref="Parse" /> method
///     returns <see cref="Node" /> and not <see cref="AnyExpr" />.
///     <code>
///         any
///             : class-def
///             | fn-def
///             | macro-def
///             | module-def
///             | if-expr
///             | while-expr
///             | decorable-expr
///             | break-expr
///             | continue-expr
///             | return-expr
///             | empty-expr
///             | scope-expr
///             | infix
///             | var-def;
///         var-def:
///             (['let'] assignable
///             [':' type]
///             ['=' multiple-infix]);
///     </code>
/// </summary>
public static class AnyExpr {
    internal static Node Parse(Node parent) {
        var s = parent.Unit.TokenStream;

        if (s.PeekIs(KeywordClass)) {
            return new ClassDef(parent).Parse();
        }

        if (s.PeekIs(KeywordFn)) {
            return new FunctionDef(parent).Parse();
        }

        if (s.PeekIs(KeywordMacro)) {
            return new MacroDef(parent).Parse();
        }

        if (s.PeekIs(KeywordModule)) {
            return new ModuleDef(parent).Parse();
        }

        if (s.PeekIs(KeywordIf)) {
            return new IfExpr(parent).Parse();
        }

        if (s.PeekIs(KeywordWhile)) {
            return new WhileExpr(parent).Parse();
        }

        if (s.PeekIs(At)) {
            return new DecoratedExpr(parent).Parse();
        }

        if (s.PeekIs(KeywordImport)) {
            return new ImportExpr(parent).Parse();
        }

        if (s.PeekIs(KeywordBreak)) {
            return new BreakExpr(parent).Parse();
        }

        if (s.PeekIs(KeywordContinue)) {
            return new ContinueExpr(parent).Parse();
        }

        if (s.PeekIs(KeywordReturn)) {
            return new ReturnExpr(parent).Parse();
        }

        if (s.PeekIs(Semicolon, KeywordPass)) {
            return new EmptyExpr(parent).Parse();
        }

        if (s.PeekIs(Spec.ScopeStartMarks)) {
            return new ScopeExpr(parent).Parse();
        }

        var immutableKw = s.MaybeEat(KeywordLet) ? s.Token : null;
        var infix = InfixExpr.Parse(parent);

        if (infix is BinaryExpr { Operator.Type: EqualsSign } bin
         && bin.GetParent<ScopeExpr>() is { } parentScope) {
            // ['let'] name '=' expr
            // --------------------^
            if (bin.Left is NameExpr name
             && !parentScope.IsDefined(name)) {
                return new VarDef(parent, immutableKw) {
                    Name  = name,
                    Value = bin.Right
                };
            }

            if (bin.Left is TupleExpr) {
                return bin;
            }
        }

        // ['let'] name [':' type-name ['=' infix-expr]]
        // -----------^
        if (immutableKw == null && !s.MaybeEat(Colon)) {
            return infix;
        }

        // ['let'] name ':' type-name ['=' infix-expr]
        // -----------------^
        if (infix is not NameExpr varName) {
            LanguageReport.To(BlameType.ExpectedVarName, infix);
            return infix;
        }

        var type = TypeName.Parse(parent);
        Node? value = null;
        if (s.MaybeEat(EqualsSign)) // ['let'] name ':' type-name '=' infix-expr
            // -------------------------------^
        {
            value = InfixExpr.Parse(parent);
        }

        return new VarDef(parent, immutableKw) {
            Name         = varName,
            InferredType = type,
            Value        = value
        };
    }
}
