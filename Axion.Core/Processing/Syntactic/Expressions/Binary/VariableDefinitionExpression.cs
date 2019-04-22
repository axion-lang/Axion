using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    /// <summary>
    ///     <c>
    ///         var_def_expr:
    ///             ['let'] test_list [':' type] '=' test_list | yield_expr
    ///     </c>
    /// </summary>
    public class VariableDefinitionExpression : LeftRightExpression {
        private TypeName type;

        public TypeName Type {
            get => type;
            set => SetNode(ref type, value);
        }

        public bool IsImmutable { get; set; }

        public VariableDefinitionExpression(
            SyntaxTreeNode parent,
            Expression     assignable,
            TypeName?      type,
            Expression?    value
        ) : base(parent) {
            Left  = assignable;
            Type  = type;
            Right = value;
            if (ParentBlock.HasVariable((SimpleNameExpression) Left)) {
                Unit.ReportError(
                    "Cannot declare variable, because it's already declared in this scope.'",
                    this
                );
            }
            else {
                ParentBlock.Variables.Add(this);
            }
        }

        public override void ToAxionCode(CodeBuilder c) {
            if (IsImmutable) {
                c.Write("let ");
            }

            c.Write(Left);
            if (Type != null) {
                c.Write(": ", Type);
            }

            if (Right != null) {
                c.Write(" = ", Right);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            if (Right == null) {
                c.Write(Type, " ", Left);
            }
            else {
                c.Write("var ", Left, " = ", Right);
            }
        }
    }
}