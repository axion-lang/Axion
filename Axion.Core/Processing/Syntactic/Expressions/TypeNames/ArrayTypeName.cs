using Axion.Core.Processing.Lexical.Tokens;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <code>
    ///         array-type:
    ///             type '[' ']';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class ArrayTypeName : TypeName {
        [LeafSyntaxNode] TypeName? elementType;

        [LeafSyntaxNode] Token? openingBracket;

        [LeafSyntaxNode] Token? closingBracket;

        public ArrayTypeName(Node parent) : base(parent) { }

        public ArrayTypeName Parse() {
            ElementType ??= Parse(this);

            OpeningBracket = Stream.Eat(OpenBracket);
            ClosingBracket = Stream.Eat(CloseBracket);
            return this;
        }
    }
}
