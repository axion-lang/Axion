using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class GeneratorExpression : Expression {
        private ForComprehension comprehension;

        public ForComprehension Comprehension {
            get => comprehension;
            set => SetNode(ref comprehension, value);
        }

        internal override TypeName ValueType => Comprehension.Parent.ValueType;

        public GeneratorExpression(SyntaxTreeNode parent, ForComprehension comprehension) : base(
            parent
        ) {
            Comprehension = comprehension;
        }

        public GeneratorExpression(ForComprehension comprehension) {
            Comprehension = comprehension;
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("(", Comprehension, ")");
        }

        public override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}