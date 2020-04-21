using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    /// <summary>
    ///     <c>
    ///         or-pattern:
    ///             syntax-pattern '|' syntax-pattern;
    ///     </c>
    /// </summary>
    public class OrPattern : Pattern {
        private Pattern left = null!;

        public Pattern Left {
            get => left;
            set => left = Bind(value);
        }

        private Pattern right = null!;

        public Pattern Right {
            get => right;
            set => right = Bind(value);
        }

        public OrPattern(Node parent) : base(parent) { }

        public override bool Match(Node parent) {
            return Left.Match(parent) || Right.Match(parent);
        }

        public OrPattern Parse() {
            Left ??= new CascadePattern(this).Parse();
            Stream.Eat(OpBitOr);
            Right = new CascadePattern(this).Parse();
            return this;
        }
    }
}
