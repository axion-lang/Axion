using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

/// <summary>
///     <code>
///         return-expr:
///             'return' [multiple-expr];
///     </code>
/// </summary>
[Branch]
public partial class ReturnExpr : Node {
    [Leaf] Token? kwReturn;
    [Leaf] Node? value;

    public override TypeName? InferredType => Value?.InferredType;

    public ReturnExpr(Node parent) : base(parent) { }

    public ReturnExpr Parse() {
        KwReturn = Stream.Eat(KeywordReturn);
        if (!Stream.PeekIs(Spec.NeverExprStartTypes)) {
            Value = Multiple.ParsePermissively<InfixExpr>(this);
        }

        return this;
    }
}
