using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <code>
    ///         group-pattern:
    ///             '(' syntax-pattern ')';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class GroupPattern : Pattern {
        [LeafSyntaxNode] Pattern pattern = null!;

        public GroupPattern(Node parent) : base(parent) { }

        public override bool Match(MacroMatchExpr parent) {
            return Pattern.Match(parent);
        }

        public GroupPattern Parse() {
            Stream.Eat(OpenParenthesis);
            Pattern = new CascadePattern(this).Parse();
            Stream.Eat(CloseParenthesis);
            return this;
        }
    }
}
