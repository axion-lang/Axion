using System;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
    public class UnknownExpression : Expression {
        public UnknownExpression(Expression parent) {
            Construct(parent, () => {
                while (!Peek.Is(Newline, End)) {
                    Eat();
                }
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}