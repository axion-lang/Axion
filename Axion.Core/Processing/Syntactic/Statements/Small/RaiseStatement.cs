using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         raise_stmt:
    ///             'raise' [preglobal_expr ['from' preglobal_expr]]
    ///     </c>
    /// </summary>
    public class RaiseStatement : Statement {
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
        internal RaiseStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordRaise);

            if (!Peek.Is(Spec.NeverExprStartTypes)) {
                Exception = Expression.ParsePreGlobalExpr(this);
                if (MaybeEat(KeywordFrom)) {
                    Cause = Expression.ParsePreGlobalExpr(this);
                }
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public RaiseStatement(Expression exception, Expression cause) {
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