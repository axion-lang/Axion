using Axion.Core.Processing.Lexical.Tokens;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <code>
    ///         union-type:
    ///             type '|' type;
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class UnionTypeName : TypeName {
        [LeafSyntaxNode] TypeName? left;
        [LeafSyntaxNode] Token? joiningMark;
        [LeafSyntaxNode] TypeName? right;

        public UnionTypeName(Node parent) : base(parent) { }

        public UnionTypeName Parse() {
            Left ??= Parse(this);

            JoiningMark = Stream.Eat(Pipe);
            Right       = Parse(this);
            return this;
        }
    }
}
