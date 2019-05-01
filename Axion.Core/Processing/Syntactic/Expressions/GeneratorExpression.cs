using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         generator_expr:
    ///             '(' comprehension ')'
    ///     </c>
    /// </summary>
    public class GeneratorExpression : Expression {
        private ForComprehension comprehension;

        public ForComprehension Comprehension {
            get => comprehension;
            set => SetNode(ref comprehension, value);
        }

        public override TypeName ValueType => Comprehension.ValueType;

        public GeneratorExpression(SyntaxTreeNode parent, ForComprehension comprehension) : base(
            parent
        ) {
            Comprehension = comprehension;
        }

        public GeneratorExpression(ForComprehension comprehension) {
            Comprehension = comprehension;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(", Comprehension, ")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}