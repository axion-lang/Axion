using Axion.Core.Processing.Lexical.Tokens;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <code>
    ///         generic-type:
    ///             type '[' type {',' type} ']';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class GenericTypeName : TypeName {
        [LeafSyntaxNode] TypeName? target;
        [LeafSyntaxNode] Token? typeArgsStartMark;
        [LeafSyntaxNode] NodeList<TypeName>? typeArgs;
        [LeafSyntaxNode] Token? typeArgsEndMark;

        public GenericTypeName(Node parent) : base(parent) { }

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
}
