using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         variable-definition-expr:
    ///             ['let'] simple-multiple-name
    ///             [':' type]
    ///             ['=' multiple-expr];
    ///     </c>
    /// </summary>
    public class VarDef : NameDef {
        public bool IsImmutable { get; }

        public VarDef(
            Expr?     parent      = null,
            Span?     kwImmutable = null,
            NameExpr? name        = null,
            TypeName? type        = null,
            Expr?     value       = null
        ) : base(parent, name, type, value) {
            IsImmutable = kwImmutable != null;

            MarkStart(kwImmutable   ?? name);
            MarkEnd((value ?? type) ?? name);
        }

        public override void ToAxion(CodeWriter c) {
            if (IsImmutable) {
                c.Write("let ");
            }

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
    }
}