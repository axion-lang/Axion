using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <code>
    ///         multiple-pattern:
    ///             '{' syntax-pattern '}'
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class MultiplePattern : Pattern {
        [LeafSyntaxNode] Pattern pattern = null!;

        public MultiplePattern(Node parent) : base(parent) { }

        public override bool Match(MacroMatchExpr parent) {
            var matchCount = 0;
            while (Pattern.Match(parent)) {
                matchCount++;
            }

            return matchCount > 0;
        }

        public MultiplePattern Parse() {
            Stream.Eat(OpenBrace);
            Pattern = new CascadePattern(this).Parse();
            Stream.Eat(CloseBrace);
            return this;
        }
    }
}
