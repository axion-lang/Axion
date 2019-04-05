using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.Binary {
    /// <summary>
    ///     <c>
    ///         bin_expr ::=
    ///             expr OPERATOR expr
    ///     </c>
    /// </summary>
    public class BinaryExpression : LeftRightExpression {
        [NotNull]
        public readonly OperatorToken Operator;

        public BinaryExpression(
            [NotNull] Expression    left,
            [NotNull] OperatorToken op,
            [NotNull] Expression    right
        ) {
            Left     = left;
            Operator = op;
            Right    = right;

            MarkPosition(Left, Right);
        }

        public BinaryExpression(
            [NotNull] Expression left,
            TokenType            opType,
            [NotNull] Expression right
        ) : this(
            left,
            new OperatorToken(opType),
            right
        ) { }

        public BinaryExpression(
            [NotNull] Expression left,
            [NotNull] string     op,
            [NotNull] Expression right
        ) : this(
            left,
            new OperatorToken(op),
            right
        ) { }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Left + " " + Operator.Value + " " + Right;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            if (Spec.CSharp.BinaryOperators.TryGetValue(Operator.Type, out string op)) {
                return c + Left + op + Right;
            }

            Unit.ReportError("This operator is not implemented in C#.", Operator);
            return null;
        }
    }
}