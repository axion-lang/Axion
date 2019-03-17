using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class CompoundAssignExpression : LeftRightExpression {
        public SymbolToken Operator { get; }

        public CompoundAssignExpression(Expression left, SymbolToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));
        }

        public CompoundAssignExpression(Expression left, TokenType opType, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = new SymbolToken(opType);
            Right    = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }

        public CompoundAssignExpression(Expression left, string op, Expression right) {
            if (op == null) {
                throw new ArgumentNullException(nameof(op));
            }

            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = new SymbolToken(op);
            Right    = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Left + " " + Operator.Value + " " + Right;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            if (Spec.CSharp.CompoundAssignmentOperators.TryGetValue(Operator.Type, out string op)) {
                return c + Left + " " + op + " " + Right;
            }

            SourceUnit.ReportError("This operator is not implemented in C#.", Operator);
            return null;
        }
    }
}