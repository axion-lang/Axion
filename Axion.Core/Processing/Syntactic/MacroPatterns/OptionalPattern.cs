namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public class OptionalPattern : IPattern {
        internal readonly IPattern Pattern;

        public OptionalPattern(IPattern pattern) {
            Pattern = pattern;
        }

        public OptionalPattern(params IPattern[] patterns) {
            Pattern = new CascadePattern(patterns);
        }

        public bool Match(Expression parent) {
            Pattern.Match(parent);
            return true;
        }
    }
}