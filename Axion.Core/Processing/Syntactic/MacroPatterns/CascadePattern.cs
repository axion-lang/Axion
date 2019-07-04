namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public class CascadePattern : IPattern {
        internal readonly IPattern[] Patterns;

        public CascadePattern(params IPattern[] patterns) {
            Patterns = patterns;
        }

        public bool Match(Expression parent) {
            foreach (IPattern pattern in Patterns) {
                if (!pattern.Match(parent)) {
                    return false;
                }
            }

            return true;
        }
    }
}