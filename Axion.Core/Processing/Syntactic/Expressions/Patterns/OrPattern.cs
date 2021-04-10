using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         or-pattern:
    ///             syntax-pattern '|' syntax-pattern;
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class OrPattern : Pattern {
        [LeafSyntaxNode] Pattern? left;
        [LeafSyntaxNode] Pattern right = null!;

        public OrPattern(Node parent) : base(parent) { }

        public override bool Match(MacroMatchExpr parent) {
            return (Left?.Match(parent) ?? false) || Right.Match(parent);
        }

        public OrPattern Parse() {
            Left ??= new CascadePattern(this).Parse();
            Stream.Eat(Pipe);
            Right = new CascadePattern(this).Parse();
            return this;
        }
    }
}
