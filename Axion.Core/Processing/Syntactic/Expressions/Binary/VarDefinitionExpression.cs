using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    /// <summary>
    ///     <c>
    ///         var_def_expr ::=
    ///             ['let'] test_list [':' type] '=' test_list | yield_expr
    ///     </c>
    /// </summary>
    public class VarDefinitionExpression : LeftRightExpression {
        private TypeName type;

        public TypeName Type {
            get => type;
            set => SetNode(ref type, value);
        }

        public bool IsImmutable { get; set; }

        public VarDefinitionExpression([NotNull] Expression left, TypeName type, Expression right) {
            Left  = left;
            Type  = type;
            Right = right;
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + Left + ": " + Type;
            if (Right != null) {
                c = c + " = " + Right;
            }

            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + Type + " " + Left;
            if (Right != null) {
                c = c + "=" + Right;
            }

            return c;
        }
    }
}