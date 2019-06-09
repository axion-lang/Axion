using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class UnaryExpression : Expression {
        public readonly OperatorToken Operator;
        private         Expression    val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public UnaryExpression(
            AstNode       parent,
            OperatorToken op,
            Expression    expression
        ) : base(parent) {
            MarkStart(Operator = op);
            MarkEnd(Value      = expression);
        }

        public UnaryExpression(
            OperatorToken op,
            Expression    expression
        ) {
            MarkStart(Operator = op);
            MarkEnd(Value      = expression);
        }

        public UnaryExpression(
            AstNode    parent,
            TokenType  opType,
            Expression expression
        ) : base(parent) {
            MarkStart(Operator = new OperatorToken(opType));
            MarkEnd(Value      = expression);
        }

        public UnaryExpression(
            TokenType  opType,
            Expression expression
        ) {
            Operator = new OperatorToken(opType) {
                Properties = {
                    InputSide =
                        Spec.Operators.Values.First(p => p.Type == opType).InputSide
                        != InputSide.Right
                            ? InputSide.Left
                            : InputSide.Right
                }
            };
            MarkPosition(Value = expression);
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