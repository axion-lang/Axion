using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         tuple-type:
///             '(' [type {',' type}] ')';
///     </code>
/// </summary>
[Branch]
public partial class TupleTypeName : TypeName {
    [Leaf] Token? endMark;
    [Leaf] Token? startMark;
    [Leaf] NodeList<TypeName, Ast>? types;

    public TupleTypeName(Node parent) : base(parent) { }

    public TupleTypeName Parse() {
        StartMark = Stream.Eat(OpenParenthesis);
        if (!Stream.PeekIs(CloseParenthesis)) {
            do {
                Types += Parse(this);
            } while (Stream.MaybeEat(Comma));
        }

        EndMark = Stream.Eat(CloseParenthesis);
        return this;
    }
}
