using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         assert_stmt ::=
    ///             'assert' test [',' test]
    ///     </c>
    /// </summary>
    public class AssertStatement : Statement {
        private Expression condition;

        [NotNull]
        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private Expression failExpression;

        public Expression FailExpression {
            get => failExpression;
            set => SetNode(ref failExpression, value);
        }

        public AssertStatement(
            [NotNull] Expression condition,
            Expression           failExpression
        ) {
            Condition      = condition;
            FailExpression = failExpression;

            MarkEnd(failExpression ?? condition);
        }

        internal AssertStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordAssert);

            Condition = Expression.ParseTestExpr(this);
            if (MaybeEat(TokenType.Comma)) {
                FailExpression = Expression.ParseTestExpr(this);
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + "assert " + Condition;
            if (FailExpression != null) {
                c = c + ", " + FailExpression;
            }

            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + "Debug.Assert(" + Condition;
            if (FailExpression != null) {
                c = c + "," + FailExpression;
            }

            return c + ");";
        }
    }
}