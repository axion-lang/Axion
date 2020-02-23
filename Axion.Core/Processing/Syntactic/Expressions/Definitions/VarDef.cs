using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         variable_definition_expr:
    ///             ['let'] simple_name_list
    ///             [':' type]
    ///             ['=' expr_list];
    ///     </c>
    /// </summary>
    public class VarDef : NameDef, IStatementExpr {
        public bool IsImmutable { get; }

        public VarDef(
            Expr     parent    = null,
            NameExpr name      = null,
            TypeName type      = null,
            Expr     value     = null,
            bool     immutable = false
        ) : base(parent, name, type, value) {
            IsImmutable = immutable;
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