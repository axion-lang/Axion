using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.SourceGenerators;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    [SyntaxExpression]
    public partial class NameDef : Node, IDefinitionExpr, IDecorableExpr {
        [LeafSyntaxNode] NameExpr? name;
        [LeafSyntaxNode] Node? value;

        public NameDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Node[] items) {
            return new(Parent) {
                Target = this,
                Decorators = new NodeList<Node>(this, items)
            };
        }

        // TODO: all name references as property
    }
}
