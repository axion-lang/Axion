using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         var_expr
    ///             : infix_list
    ///             | (['let'] assignable
    ///                [':' type]
    ///                ['=' infix_list]);
    ///     </c>
    /// </summary>
    public static class AnyExpr {
        internal static Expr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            switch (s.Peek.Type) {
            case KeywordModule: {
                return new ModuleDef(parent).Parse();
            }

            case KeywordClass: {
                return new ClassDef(parent).Parse();
            }

            case KeywordFn: {
                return new FunctionDef(parent).Parse();
            }

            case KeywordMacro: {
                return new MacroDef(parent).Parse();
            }

            case KeywordIf: {
                return new ConditionalExpr(parent).Parse();
            }

            case KeywordWhile: {
                return new WhileExpr(parent).Parse();
            }

            case At: {
                return new DecoratedExpr(parent).Parse();
            }

            case Semicolon:
            case KeywordPass: {
                return new EmptyExpr(parent).Parse();
            }

            case Indent:
            case OpenBrace:
            case Colon
                when parent.Ast.MacroExpectationType == typeof(BlockExpr): {
                return new BlockExpr(parent).Parse();
            }

            case KeywordBreak: {
                return new BreakExpr(parent).Parse();
            }

            case KeywordContinue: {
                return new ContinueExpr(parent).Parse();
            }

            case KeywordReturn: {
                return new ReturnExpr(parent).Parse();
            }

            case KeywordYield: {
                return new YieldExpr(parent).Parse();
            }
            }

            bool isImmutable = s.MaybeEat(KeywordLet);

            Expr expr = InfixExpr.Parse(parent);

            if (expr is BinaryExpr bin && bin.Operator.Is(OpAssign)) {
                // ['let'] name '=' expr
                // --------------------^
                if (bin.Left is NameExpr name
                 && !bin.GetParentOfType<BlockExpr>().IsDefined(name.ToString())) {
                    return new VarDef(
                        parent,
                        name,
                        null,
                        bin.Right,
                        isImmutable
                    );
                }

                if (bin.Left is TupleExpr) {
                    return bin;
                }
            }

            // ['let'] name [':' type_name ['=' infix_expr]]
            // -----------^
            if (!isImmutable && !s.MaybeEat(Colon)) {
                return expr;
            }

            // ['let'] name ':' type_name ['=' infix_expr]
            // -----------------^
            if (!(expr is NameExpr varName)) {
                LangException.Report(BlameType.ExpectedVarName, expr);
                varName = null;
            }

            TypeName type  = new TypeName(parent).ParseTypeName();
            Expr     value = null;
            if (s.MaybeEat(OpAssign)) {
                // ['let'] name ':' type_name '=' infix_expr
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