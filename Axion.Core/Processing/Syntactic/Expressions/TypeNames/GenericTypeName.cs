using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         generic-type:
///             type '[' type {',' type} ']';
///     </code>
/// </summary>
[Branch]
public partial class GenericTypeName : TypeName {
    [Leaf] TypeName? target;
    [Leaf] NodeList<TypeName, Ast>? typeArgs;
    [Leaf] Token? typeArgsEndMark;
    [Leaf] Token? typeArgsStartMark;

    public GenericTypeName(Node? parent) : base(parent) { }

    public GenericTypeName Parse() {
        Target ??= Parse(this);

        TypeArgsStartMark = Stream.Eat(OpenBracket);
        do {
            TypeArgs += Parse(this);
        } while (Stream.MaybeEat(Comma));

        TypeArgsEndMark = Stream.Eat(CloseBracket);
        return this;
    }
}
