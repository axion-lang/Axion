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

        public GeneratorExpression(ForComprehension comprehension) {
            Parent        = comprehension.Parent;
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