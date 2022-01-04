using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         union-type:
///             type '|' type;
///     </code>
/// </summary>
[Branch]
public partial class UnionTypeName : TypeName {
    [Leaf] Token? joiningMark;
    [Leaf] TypeName? left;
    [Leaf] TypeName? right;

    public UnionTypeName(Node parent) : base(parent) { }

    public UnionTypeName Parse() {
        Left ??= Parse(this);

        JoiningMark = Stream.Eat(Pipe);
        Right       = Parse(this);
        return this;
    }
}
