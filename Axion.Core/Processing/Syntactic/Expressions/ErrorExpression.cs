using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class ErrorExpression : Expression {
        public ErrorExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(Token);
            while (!Peek.Is(TokenType.Newline, TokenType.End)) {
                Move();
            }

            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}