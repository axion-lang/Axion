namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public interface IPattern {
        bool Match(Expression parent);
    }
}