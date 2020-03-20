using System;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common {
    /// <summary>
    ///     <c>
    ///         var-expr
    ///             : multiple-infix
    ///             | (['let'] assignable
    ///                [':' type]
    ///                ['=' multiple-infix]);
    ///     </c>
    /// </summary>
    public static class AnyExpr {
        internal static Expr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

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
                return new DecorableExpr(parent).Parse();
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
            if (s.PeekIs(Indent, OpenBrace, Colon)) {
                return new ScopeExpr(parent).Parse();
            }
            bool isImmutable = s.MaybeEat(KeywordLet);

            Expr expr = InfixExpr.Parse(parent);

            if (expr is BinaryExpr bin && bin.Operator.Is(OpAssign)) {
                // ['let'] name '=' expr
                // --------------------^
                if (bin.Left is NameExpr name
                 && !bin.GetParentOfType<ScopeExpr>().IsDefined(name.ToString())) {
                    return new VarDef(
                        parent,
                        name,
                        null,
                        bin.Right,
                        isImmutable
                    );
                }
                Type valueType = bin.Left.GetType();
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(TupleExpr<>)) {
                    return bin;
                }
            }

            // ['let'] name [':' type-name ['=' infix-expr]]
            // -----------^
            if (!isImmutable && !s.MaybeEat(Colon)) {
                return expr;
            }

            // ['let'] name ':' type-name ['=' infix-expr]
            // -----------------^
            if (!(expr is NameExpr varName)) {
                LangException.Report(BlameType.ExpectedVarName, expr);
                varName = null;
            }

            TypeName type  = new TypeName(parent).ParseTypeName();
            Expr     value = null;
            if (s.MaybeEat(OpAssign)) {
                // ['let'] name ':' type-name '=' infix-expr
                // -------------------------------^
                value = InfixExpr.Parse(parent);
            }

            return new VarDef(
                parent, varName, type, value,
                isImmutable
            );
        }
    }
}