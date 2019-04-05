using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         raise_stmt ::=
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

        public RaiseStatement([NotNull] Expression exception, [NotNull] Expression cause) {
            Exception = exception;
            Cause     = cause;
        }

        internal RaiseStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordRaise);

            if (!PeekIs(Spec.NeverTestTypes)) {
                Exception = Expression.ParseTestExpr(this);
                if (MaybeEat(TokenType.KeywordFrom)) {
                    Cause = Expression.ParseTestExpr(this);
                }
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c += "raise";
            if (Exception != null) {
                c = c + " " + Exception;
            }

            if (Cause != null) {
                c = c + " " + Cause;
            }

            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c += "throw new Exception(";
            if (Exception is ConstantExpression constant) {
                c = c + constant + ".ToString()";
            }

            return c + ");";
        }
    }
}