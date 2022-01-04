using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         array-type:
///             type '[' ']';
///     </code>
/// </summary>
[Branch]
public partial class ArrayTypeName : TypeName {
    [Leaf] Token? closingBracket;
    [Leaf] TypeName? elementType;
    [Leaf] Token? openingBracket;

    public ArrayTypeName(Node parent) : base(parent) { }

    public ArrayTypeName Parse() {
        ElementType ??= Parse(this);

        OpeningBracket = Stream.Eat(OpenBracket);
        ClosingBracket = Stream.Eat(CloseBracket);
        return this;
    }
}
