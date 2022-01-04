using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations;

/// <summary>
///     <code>
///         ternary-expr:
///             multiple-expr ('if' | 'unless') infix-expr ['else' multiple-expr];
///     </code>
/// </summary>
[Branch]
public partial class TernaryExpr : InfixExpr {
    [Leaf] Node? condition;
    [Leaf] Node? falseExpr;
    [Leaf] Token? falseMark;
    [Leaf] Node? trueExpr;
    [Leaf] Token? trueMark;

    public override TypeName? InferredType =>
        TrueExpr?.InferredType ?? FalseExpr?.InferredType;

    internal TernaryExpr(Node parent) : base(parent) { }

    public TernaryExpr Parse() {
        var invert = false;
        if (!Stream.MaybeEat(KeywordIf)) {
            Stream.Eat(KeywordUnless);
            invert = true;
        }

        TrueMark  =   Stream.Token;
        TrueExpr  ??= AnyExpr.Parse(this);
        Condition =   Parse(this);
        if (Stream.MaybeEat(KeywordElse)) {
            FalseMark = Stream.Token;
            FalseExpr = AnyExpr.Parse(this);
        }

        if (invert) {
            (TrueExpr, FalseExpr) = (FalseExpr!, TrueExpr);
        }

        return this;
    }
}
