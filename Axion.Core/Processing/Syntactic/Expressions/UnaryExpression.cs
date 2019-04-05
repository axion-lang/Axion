using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class UnaryExpression : Expression {
        [NotNull]
        public readonly OperatorToken Operator;

        private Expression val;

        [NotNull]
        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public UnaryExpression([NotNull] OperatorToken op, [NotNull] Expression expression) {
            Operator   = op;
            Value = expression;

            MarkPosition(op, expression);
        }

        public UnaryExpression(TokenType opType, [NotNull] Expression expression) {
            Operator   = new OperatorToken(opType);
            Value = expression;

            MarkPosition(Value);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            if (Operator.Properties.InputSide == InputSide.Left) {
                return c + Value + Operator.Value;
            }

            return c + Operator.Value + Value;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            if (Operator.Properties.InputSide == InputSide.Left) {
                return c + Value + Operator.Value;
            }

            return c + Operator.Value + Value;
        }
    }
}