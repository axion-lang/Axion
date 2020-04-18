namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    /// <summary>
    ///     <c>
    ///         syntax-pattern
    ///             : cascade-pattern
    ///             | expression-pattern
    ///             | group-pattern
    ///             | multiple-pattern
    ///             | optional-pattern
    ///             | or-pattern
    ///             | token-pattern;
    ///     </c>
    /// </summary>
    public abstract class Pattern : Expr {
        // NOTE: here Match uses parent parameter because
        // we may want to match macro from another file.
        public abstract bool Match(Expr parent);

        protected Pattern(Expr parent) : base(parent) { }
    }
}
