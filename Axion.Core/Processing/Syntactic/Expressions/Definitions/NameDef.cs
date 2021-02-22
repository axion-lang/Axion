using Axion.Core.Processing.Syntactic.Expressions.Atomic;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    public class NameDef : Node, IDefinitionExpr, IDecorableExpr {
        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private Node? val;

        public Node? Value {
            get => val;
            set => val = BindNullable(value);
        }

        public NameDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Node[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Node>(this, items)
            };
        }

        // TODO: all name references as property
    }
}
