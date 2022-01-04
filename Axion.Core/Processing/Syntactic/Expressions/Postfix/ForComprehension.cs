using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix;

/// <summary>
///     <code>
///         for-comprehension:
///             'for' multiple-name 'in' multiple-infix (('if'|'unless') condition)* [for-comprehension];
///     </code>
/// </summary>
[Branch]
public partial class ForComprehension : InfixExpr {
    [Leaf] NodeList<Node, Ast>? conditions;
    [Leaf] Node item = null!;
    [Leaf] Node iterable = null!;
    [Leaf] Node? right;
    [Leaf] [NotTraversable] Node? target;

    public bool IsNested { get; init; }

    public override TypeName? InferredType => Target?.InferredType;

    public ForComprehension(Node parent) : base(parent) { }

    public ForComprehension Parse() {
        if (Target == null && !IsNested) {
            Target = Parse(this);
        }

        Stream.Eat(KeywordFor);
        Item = Multiple.Parse<AtomExpr>(this);
        Stream.Eat(In);
        Iterable = Multiple.Parse<InfixExpr>(this);

        while (Stream.PeekIs(KeywordIf, KeywordUnless)) {
            if (Stream.MaybeEat(KeywordIf)) {
                Conditions += Parse(this);
            }
            else if (Stream.MaybeEat(KeywordUnless)) {
                Conditions += new UnaryExpr(this) {
                    Operator = new OperatorToken(
                        Unit,
                        tokenType: Not
                    ),
                    Value = Parse(this)
                };
            }
        }

        if (Stream.PeekIs(KeywordFor)) {
            Right = new ForComprehension(Parent!) {
                Target   = this,
                IsNested = true
            }.Parse();
        }
        return this;
    }
}
