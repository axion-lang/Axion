using Axion.Core.Processing.Syntactic.Expressions.Atomic;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple-type:
    ///             name;
    ///     </c>
    /// </summary>
    public class SimpleTypeName : TypeName, ITypeParameter {
        private NameExpr name = null!;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        public SimpleTypeName(Node parent) : base(parent) { }

        public SimpleTypeName(Node parent, string name) : base(parent) {
            Name = new NameExpr(this, name);
        }

        public SimpleTypeName Parse() {
            Name = new NameExpr(this).Parse();
            return this;
        }
    }
}
