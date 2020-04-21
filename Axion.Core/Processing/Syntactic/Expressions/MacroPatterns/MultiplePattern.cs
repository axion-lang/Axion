using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    /// <summary>
    ///     <c>
    ///         multiple-pattern:
    ///             '{' syntax-pattern '}'
    ///     </c>
    /// </summary>
    public class MultiplePattern : Pattern {
        private Pattern pattern = null!;

        public Pattern Pattern {
            get => pattern;
            set => pattern = Bind(value);
        }

        public MultiplePattern(Node parent) : base(parent) { }

        public override bool Match(Node parent) {
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
