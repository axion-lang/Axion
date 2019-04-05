using System.Diagnostics;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class ConstantExpression : Expression {
        public            Token  Value              { get; }
        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        internal ConstantExpression(TokenType type) {
            Debug.Assert(Spec.Keywords.ContainsValue(type));
            Value = new WordToken(type);
        }

        internal ConstantExpression(Token value) {
            Value = value;
            MarkPosition(Value);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Value;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + Value;
        }
    }
}