using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    public class NameDef : Expr, IDefinitionExpr, IDecorableExpr {
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private Expr? val;

        public Expr? Value {
            get => val;
            set => val = Bind(value);
        }

        private TypeName valueType;

        public sealed override TypeName ValueType {
            get => valueType /* BUG here ?? Right.ValueType*/;
            protected set => valueType = Bind(value);
        }

        public NameDef(
            Expr?     parent = null,
            NameExpr? name   = null,
            TypeName? type   = null,
            Expr?     value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(name, type, value)
        ) {
            Name      = name;
            ValueType = type;
            Value     = value;
        }

        // TODO: all name references as property
    }
}