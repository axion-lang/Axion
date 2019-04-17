using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class AwaitExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal override TypeName ValueType => Value.ValueType;

        public AwaitExpression(Expression value) {
            Value = value;
        }

        internal AwaitExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordAwait);

            // TODO: add in 'async' context check
            Value = ParseMultiple(parent, expectedTypes: Spec.TestExprs);

            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("await ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("await ", Value);
        }
    }
}