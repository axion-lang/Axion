using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;
using Magnolia.Trees;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions;

[Branch]
public partial class NameDef : Node, IDefinitionExpr, IDecorableExpr {
    [Leaf] NameExpr? name;
    [Leaf] Node? value;

    public NameDef(Node parent) : base(parent) { }

    public DecoratedExpr WithDecorators(params Node[] items) {
        return new(Parent) {
            Target     = this,
            Decorators = new NodeList<Node, Ast>(this, items)
        };
    }

    // TODO: all name references as property
}
