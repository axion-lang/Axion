using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         or-pattern:
    ///             syntax-pattern '|' syntax-pattern;
    ///     </c>
    /// </summary>
    public class OrPattern : Pattern {
        private Pattern? left;

        public Pattern? Left {
            get => left;
            set => left = BindNullable(value);
        }

        private Pattern right = null!;

        public Pattern Right {
            get => right;
            set => right = Bind(value);
        }

        public OrPattern(Node parent) : base(parent) { }

        public override bool Match(Expr parent) {
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
