using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         raise_stmt:
    ///             'raise' [test ['from' test]]
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

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="RaiseStatement"/> from tokens.
        /// </summary>
        internal RaiseStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordRaise);

            if (!Peek.Is(Spec.NeverTestTypes)) {
                Exception = Expression.ParseTestExpr(this);
                if (MaybeEat(TokenType.KeywordFrom)) {
                    Cause = Expression.ParseTestExpr(this);
                }
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="RaiseStatement"/> without position in source.
        /// </summary>
        public RaiseStatement(Expression exception, Expression cause) {
            Exception = exception;
            Cause     = cause;
        }

        #endregion

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("raise");
            if (Exception != null) {
                c.Write(" ", Exception);
            }

            if (Cause != null) {
                c.Write(" ", Cause);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("throw new Exception(");
            if (Exception is ConstantExpression constant) {
                c.Write(constant, ".ToString()");
            }

            c.Write(");");
        }

        #endregion
    }
}