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

            // ['let'] name '=' expr
            if (expr is BinaryExpr bin
             && bin.Left is NameExpr name
             && bin.Operator.Is(OpAssign)
             && !bin.GetParentOfType<BlockExpr>().IsDefined(name.ToString())) {
                return new VarDef(
                    parent,
                    name,
                    null,
                    bin.Right,
                    isImmutable
                );
            }

            // check for ':' - starting block instead of var definition
            if (!isImmutable && !s.MaybeEat(Colon)) {
                return expr;
            }

            if (!(expr is IVarTargetExpr)) {
                LangException.Report(BlameType.RedundantEmptyListOfTypeArguments, expr);
            }

            TypeName type  = new TypeName(parent).ParseTypeName();
            Expr     value = null;
            if (s.MaybeEat(OpAssign)) {
                value = InfixExpr.Parse(parent);
            }

            return new VarDef(parent, expr, type, value, isImmutable);
        }
    }
}