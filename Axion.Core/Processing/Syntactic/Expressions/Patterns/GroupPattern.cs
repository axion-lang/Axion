using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         group-pattern:
    ///             '(' syntax-pattern ')';
    ///     </c>
    /// </summary>
    public class GroupPattern : Pattern {
        private Pattern pattern = null!;

        public Pattern Pattern {
            get => pattern;
            set => pattern = Bind(value);
        }

        public GroupPattern(Node parent) : base(parent) { }

        public override bool Match(Expr parent) {
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
