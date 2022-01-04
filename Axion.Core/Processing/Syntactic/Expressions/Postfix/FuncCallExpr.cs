using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix;

/// <summary>
///     <code>
///         func-call-expr:
///             atom '(' [multiple-arg | (arg for-comprehension)] ')';
///     </code>
/// </summary>
[Branch]
public partial class FuncCallExpr : PostfixExpr {
    [Leaf] NodeList<FuncCallArg, Ast>? args;
    [Leaf] Node target = null!;

    public FuncCallExpr(Node? parent) : base(parent) { }

    public FuncCallExpr Parse(bool allowGenerator = false) {
        Stream.Eat(OpenParenthesis);
        Args = FuncCallArg.ParseArgList(
            this,
            allowGenerator: allowGenerator
        );
        End = Stream.Eat(CloseParenthesis).End;
        return this;
    }
}
