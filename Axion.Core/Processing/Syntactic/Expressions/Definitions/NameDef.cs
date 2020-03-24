using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    public class NameDef : Expr, IDefinitionExpr, IDecorableExpr {
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => name = BindNode(value);
        }

        private Expr val;

        public Expr Value {
            get => val;
            set => val = BindNode(value);
        }

        private TypeName valueType;

        public sealed override TypeName ValueType {
            get => valueType /* BUG here ?? Right.ValueType*/;
            protected set => valueType = BindNode(value);
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

        public override void ToAxion(CodeWriter c) {
            c.Write(Name);
            if (ValueType != null) {
                c.Write(": ", ValueType);
            }

            if (Value != null) {
                c.Write(" = ", Value);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            if (Value == null) {
                c.Write(ValueType, " ", Name);
            }
            else {
                c.Write(
                    (object) ValueType ?? "var", " ", Name, " = ",
                    Value
                );
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Name);
            if (ValueType != null) {
                c.Write(": ", ValueType);
            }

            if (Value != null) {
                c.Write(" = ", Value);
            }
        }
    }
}