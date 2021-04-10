using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.SourceGenerators;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple-type:
    ///             name;
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class SimpleTypeName : TypeName, ITypeParameter {
        [LeafSyntaxNode] NameExpr name = null!;

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
