namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         variable-definition-expr:
    ///             ['let'] simple-multiple-name
    ///             [':' type]
    ///             ['=' multiple-expr];
    ///     </c>
    /// </summary>
    public class VarDef : NameDef {
        public bool IsImmutable { get; }

        public VarDef(Expr parent, Span? kwImmutable = null) : base(parent) {
            IsImmutable = kwImmutable != null;
        }
    }
}
