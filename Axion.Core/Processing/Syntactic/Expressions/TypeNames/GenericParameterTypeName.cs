using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    [SyntaxExpression]
    public partial class GenericParameterTypeName : TypeName, ITypeParameter {
        [LeafSyntaxNode] NameExpr name = null!;
        [LeafSyntaxNode] Token? typeConstraintsStartMark;
        [LeafSyntaxNode] NodeList<TypeName>? typeConstraints;

        public GenericParameterTypeName(Node parent) : base(parent) { }

        public GenericParameterTypeName Parse() {
            TypeConstraintsStartMark = Stream.Eat(Colon);
            do {
                TypeConstraints += Parse(this);
            } while (Stream.MaybeEat(Comma));
            return this;
        }
    }
}
