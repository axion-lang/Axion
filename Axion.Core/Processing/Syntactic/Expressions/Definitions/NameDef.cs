using Axion.Core.Processing.Syntactic.Expressions.Atomic;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    public class NameDef : Expr, IDefinitionExpr, IDecorableExpr {
        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private Expr? val;

        public Expr? Value {
            get => val;
            set => val = BindNullable(value);
        }

        public NameDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Expr[] items) {
            return new DecoratedExpr(Parent) {
                Target     = this,
                Decorators = new NodeList<Expr>(this, items)
            };
        }

        // TODO: all name references as property
    }
}
