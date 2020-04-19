using Axion.Core.Processing.Syntactic.Expressions.Atomic;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple-type:
    ///             name;
    ///     </c>
    /// </summary>
    public class SimpleTypeName : TypeName {
        private NameExpr name = null!;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        public SimpleTypeName(Node parent) : base(parent) { }

        public SimpleTypeName(string name) {
            Name = new NameExpr(name);
        }

        public SimpleTypeName Parse() {
            Name = new NameExpr(this).Parse();
            return this;
        }

    }
}
