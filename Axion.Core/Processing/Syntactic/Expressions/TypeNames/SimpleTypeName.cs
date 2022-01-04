using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         simple-type:
///             name;
///     </code>
/// </summary>
[Branch]
public partial class SimpleTypeName : TypeName, ITypeParameter {
    [Leaf] NameExpr name = null!;

    public SimpleTypeName(Node parent) : base(parent) { }

    public SimpleTypeName(Node parent, string name) : base(parent) {
        Name = new NameExpr(this, name);
    }

    public SimpleTypeName Parse() {
        Name = new NameExpr(this).Parse();
        return this;
    }
}
