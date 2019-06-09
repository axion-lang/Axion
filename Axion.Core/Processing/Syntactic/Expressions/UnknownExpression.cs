using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class UnknownExpression : Expression {
        public UnknownExpression(AstNode parent) : base(parent) {
            MarkStart();
            while (!Peek.Is(TokenType.Newline, TokenType.End)) {
                GetNext();
            }
            MarkEnd();
        }

        public UnknownExpression(AstNode parent, int startIdx) : base(parent) {
            int endIdx = Ast.Index;
            parent.MoveTo(startIdx);
            MarkStart();
            parent.MoveTo(endIdx);
            MarkEnd();
        }

        internal override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}