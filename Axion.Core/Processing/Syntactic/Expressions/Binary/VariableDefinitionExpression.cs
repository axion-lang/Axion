using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    /// <summary>
    ///     <c>
    ///         variable_definition_expr:
    ///             ['let'] simple_name_list
    ///             [':' type]
    ///             ['=' expr_list];
    ///     </c>
    /// </summary>
    public class VariableDefinitionExpression : LeftRightExpression {
        private TypeName valueType;

        public sealed override TypeName ValueType {
            get => valueType /* BUG here ?? Right?.ValueType*/;
            set => SetNode(ref valueType, value);
        }

        public bool IsImmutable { get; }

        public VariableDefinitionExpression(
            SyntaxTreeNode parent,
            Expression     assignable,
            TypeName?      type,
            Expression?    value,
            bool           immutable = false
        ) : base(parent) {
            Left        = assignable;
            ValueType   = type;
            Right       = value;
            IsImmutable = immutable;
            if (ParentBlock.HasVariable((SimpleNameExpression) Left)) {
                Unit.Blame(BlameType.CannotRedeclareVariableAlreadyDeclaredInThisScope, this);
            }
            else {
                ParentBlock.Variables.Add(this);
            }
        }

        internal override void ToAxionCode(CodeBuilder c) {
            if (IsImmutable) {
                c.Write("let ");
            }

            c.Write(Left);
            if (ValueType != null) {
                c.Write(": ", ValueType);
            }

            if (Right != null) {
                c.Write(" = ", Right);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (Right == null) {
                c.Write(ValueType, " ", Left);
            }
            else {
                c.Write((object) ValueType ?? "var", " ", Left, " = ", Right);
            }
        }
    }
}