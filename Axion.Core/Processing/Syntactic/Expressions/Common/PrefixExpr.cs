using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Common;

/// <summary>
///     "Prefix" expression is any <see cref="PostfixExpr" />
///     coming after any count of allowed prefix operators.
///     <br />
///     (e.g ++++++!x is valid expression)
///     <code>
///         prefix-expr:
///             (PREFIX-OPERATOR prefix) | postfix;
///     </code>
/// </summary>
public class PrefixExpr : InfixExpr {
    protected PrefixExpr(Node? parent) : base(parent) { }

    internal new static PrefixExpr Parse(Node parent) {
        var s = parent.Unit.TokenStream;

        if (s.MaybeEat(Spec.PrefixOperators)) {
            var op = (OperatorToken) s.Token;
            op.Side = InputSide.Right;
            return new UnaryExpr(parent) {
                Operator = op,
                Value    = Parse(parent)
            };
        }

        return PostfixExpr.Parse(parent);
    }
}
