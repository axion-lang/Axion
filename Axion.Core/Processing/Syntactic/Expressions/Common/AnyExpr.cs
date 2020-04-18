using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
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
            Token? immutableKw = s.MaybeEat(KeywordLet) ? s.Token : null;

            Expr expr = InfixExpr.Parse(parent);

            if (expr is BinaryExpr bin && bin.Operator.Is(OpAssign)) {
                // ['let'] name '=' expr
                // --------------------^
                if (bin.Left is NameExpr name && !bin.GetParent<ScopeExpr>().IsDefined(name)) {
                    return new VarDef(parent, immutableKw) {
                        Name = name, Value = bin.Right
                    };
                }
                if (bin.Left is TupleExpr) {
                    return bin;
                }
            }

            // ['let'] name [':' type-name ['=' infix-expr]]
            // -----------^
            if (immutableKw == null && !s.MaybeEat(Colon)) {
                return expr;
            }

            // ['let'] name ':' type-name ['=' infix-expr]
            // -----------------^
            if (!(expr is NameExpr varName)) {
                LangException.Report(BlameType.ExpectedVarName, expr);
                return expr;
            }

            TypeName type  = TypeName.Parse(parent);
            Expr?    value = null;
            if (s.MaybeEat(OpAssign)) {
                // ['let'] name ':' type-name '=' infix-expr
                // -------------------------------^
                value = InfixExpr.Parse(parent);
            }

            return new VarDef(parent, immutableKw) {
                Name = varName, ValueType = type, Value = value
            };
        }
    }
}
