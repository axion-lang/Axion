using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

[Branch]
public partial class GenericParameterTypeName : TypeName, ITypeParameter {
    [Leaf] NameExpr name = null!;
    [Leaf] NodeList<TypeName, Ast>? typeConstraints;
    [Leaf] Token? typeConstraintsStartMark;

    public GenericParameterTypeName(Node parent) : base(parent) { }

    public GenericParameterTypeName Parse() {
        TypeConstraintsStartMark = Stream.Eat(Colon);
        do {
            TypeConstraints += Parse(this);
        } while (Stream.MaybeEat(Comma));
        return this;
    }
}
