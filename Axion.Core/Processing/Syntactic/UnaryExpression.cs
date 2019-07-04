using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic {
    public class UnaryExpression : Expression {
        public readonly OperatorToken Operator;
        private         Expression    val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public UnaryExpression(
            Expression    parent,
            OperatorToken op,
            Expression    expression
        ) : base(parent) {
            MarkStart(Operator = op);
            MarkEnd(Value      = expression);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            if (Operator.Properties.InputSide == InputSide.Right) {
                c.Write(Operator.Value, " ", Value);
            }
            else {
                c.Write(Value, " ", Operator.Value);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            string op = Operator.Value;
            if (op == "not") {
                op = "!";
            }

            if (Operator.Properties.InputSide == InputSide.Right) {
                c.Write(op, " (", Value, ")");
            }
            else {
                c.Write("(", Value, ") ", op);
            }
        }
    }
}