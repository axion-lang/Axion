using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         prefix_expr:
    ///             (PREFIX_OPERATOR prefix_expr) | suffix_expr;
    ///     </c>
    /// </summary>
    public static class PrefixExpr {
        internal static Expr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            if (s.MaybeEat(Spec.PrefixOperators)) {
                var op = (OperatorToken) s.Token;
                op.Side = InputSide.Right;
                return new UnaryExpr(parent, op, Parse(parent));
            }

            return PostfixExpr.Parse(parent);
        }
    }
}