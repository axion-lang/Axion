using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Common {
    /// <summary>
    ///     <c>
    ///         prefix-expr:
    ///             (PREFIX-OPERATOR prefix-expr) | suffix-expr;
    ///     </c>
    /// </summary>
    public class PrefixExpr : InfixExpr {
        protected PrefixExpr() { }

        protected PrefixExpr(Node parent) : base(parent) { }

        internal new static PrefixExpr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            if (s.MaybeEat(Spec.PrefixOperators)) {
                var op = (OperatorToken) s.Token;
                op.Side = InputSide.Right;
                return new UnaryExpr(parent) {
                    Operator = op, Value = Parse(parent)
                };
            }

            return PostfixExpr.Parse(parent);
        }
    }
}
