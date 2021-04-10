using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         ternary-expr:
    ///             multiple-expr ('if' | 'unless') infix-expr ['else' multiple-expr];
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class TernaryExpr : InfixExpr {
        [LeafSyntaxNode] Node? condition;
        [LeafSyntaxNode] Token? trueMark;
        [LeafSyntaxNode] Node? trueExpr;
        [LeafSyntaxNode] Token? falseMark;
        [LeafSyntaxNode] Node? falseExpr;

        public override TypeName? ValueType =>
            TrueExpr?.ValueType ?? FalseExpr?.ValueType;

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
}
