namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    public class OrPattern : IPattern {
        internal readonly IPattern Left;
        internal readonly IPattern Right;

        public OrPattern(IPattern left, IPattern right) {
            Left  = left;
            Right = right;
        }

        public bool Match(Expr parent) {
            return Left.Match(parent) || Right.Match(parent);
        }
    }
}