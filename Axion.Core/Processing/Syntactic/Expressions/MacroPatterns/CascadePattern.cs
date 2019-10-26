namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    public class CascadePattern : IPattern {
        internal readonly IPattern[] Patterns;

        public CascadePattern(params IPattern[] patterns) {
            Patterns = patterns;
        }

        public bool Match(Expr parent) {
            int startIdx = parent.Stream.TokenIdx;
            foreach (IPattern pattern in Patterns) {
                if (!pattern.Match(parent)) {
                    parent.Stream.MoveAbsolute(startIdx);
                    return false;
                }
            }

            return true;
        }
    }
}