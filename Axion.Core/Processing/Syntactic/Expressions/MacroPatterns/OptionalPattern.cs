using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    /// <summary>
    ///     <c>
    ///         optional-pattern:
    ///             '[' syntax-pattern ']';
    ///     </c>
    /// </summary>
    public class OptionalPattern : Pattern {
        private Pattern pattern = null!;

        public Pattern Pattern {
            get => pattern;
            set => pattern = Bind(value);
        }

        public OptionalPattern(Node parent) : base(parent) { }

        public override bool Match(Node parent) {
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
