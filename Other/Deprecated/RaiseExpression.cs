using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         raise_expr:
    ///             'raise' [preglobal_expr ['from' preglobal_expr]]
    ///     </c>
    /// </summary>
    public class RaiseExpression : Expression {
        private Expression exception;

        public Expression Exception {
            get => exception;
            set => SetNode(ref exception, value);
        }

        private Expression cause;

        public Expression Cause {
            get => cause;
            set => SetNode(ref cause, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal RaiseExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.KeywordRaise);

            if (!Peek.Is(Spec.NeverExprStartTypes)) {
                Exception = ParseInfixExpr(this);
                if (MaybeEat(TokenType.KeywordFrom)) {
                    Cause = ParseInfixExpr(this);
                }
            }

            MarkEnd();
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public RaiseExpression(Expression exception, Expression cause) {
            Exception = exception;
            Cause     = cause;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("raise");
            if (Exception != null) {
                c.Write(" ", Exception);
            }

            if (Cause != null) {
                c.Write(" ", Cause);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("throw new Exception(");
            if (Exception is ConstantExpression constant) {
                c.Write(constant, ".ToString()");
            }

            c.Write(");");
        }
    }
}