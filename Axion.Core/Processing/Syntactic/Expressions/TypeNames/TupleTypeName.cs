using Axion.Core.Processing.Lexical.Tokens;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <code>
    ///         tuple-type:
    ///             '(' [type {',' type}] ')';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class TupleTypeName : TypeName {
        [LeafSyntaxNode] Token? startMark;
        [LeafSyntaxNode] NodeList<TypeName>? types;
        [LeafSyntaxNode] Token? endMark;

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
}
