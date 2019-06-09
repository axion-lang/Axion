namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public class MultiplePattern : IPattern {
        internal readonly IPattern Pattern;

        public MultiplePattern(IPattern pattern) {
            Pattern = pattern;
        }

        public MultiplePattern(params IPattern[] patterns) {
            Pattern = new CascadePattern(patterns);
        }

        public bool Match(AstNode parent) {
            var matchCount = 0;
            while (Pattern.Match(parent)) {
                matchCount++;
            }
            return matchCount > 0;
        }
    }
}