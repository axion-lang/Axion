namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    public interface IPattern {
        bool Match(Expr parent);
    }
}