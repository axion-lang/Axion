using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <code>
    ///         optional-pattern:
    ///             '[' syntax-pattern ']';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class OptionalPattern : Pattern {
        [LeafSyntaxNode] Pattern pattern = null!;

        public OptionalPattern(Node parent) : base(parent) { }

        public override bool Match(MacroMatchExpr parent) {
            Pattern.Match(parent);
            return true;
        }

        public OptionalPattern Parse() {
            Stream.Eat(OpenBracket);
            Pattern = new CascadePattern(this).Parse();
            Stream.Eat(CloseBracket);
            return this;
        }
    }
}
