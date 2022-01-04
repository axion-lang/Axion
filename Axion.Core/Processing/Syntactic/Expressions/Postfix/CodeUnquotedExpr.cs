using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using Magnolia.Attributes;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix;

/// <summary>
///     <code>
///         code-unquoted-expr:
///             '$' expr;
///     </code>
/// </summary>
[Branch]
public partial class CodeUnquotedExpr : PostfixExpr {
    [Leaf] Token? startMark;
    [Leaf] Node value = null!;

    public override TypeName? InferredType => Value.InferredType;

    public CodeUnquotedExpr(Node parent) : base(parent) { }

    public CodeUnquotedExpr Parse() {
        StartMark = Stream.Eat(TokenType.Dollar);
        Value     = Parse(this);
        return this;
    }
}
