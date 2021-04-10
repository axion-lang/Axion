namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <code>
    ///         variable-definition-expr:
    ///             ['let'] simple-multiple-name
    ///             [':' type]
    ///             ['=' multiple-expr];
    ///     </code>
    /// </summary>
    public class VarDef : NameDef {
        public bool IsImmutable { get; }

        public VarDef(Node parent, Node? kwImmutable = null) : base(parent) {
            IsImmutable = kwImmutable != null;
        }
    }
}
