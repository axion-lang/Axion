using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         assert_stmt:
    ///             'assert' preglobal_expr [',' preglobal_expr]
    ///     </c>
    /// </summary>
    public class AssertStatement : Statement {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private Expression failExpression;

        public Expression FailExpression {
            get => failExpression;
            set => SetNode(ref failExpression, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal AssertStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordAssert);

            Condition = Expression.ParsePreGlobalExpr(this);
            if (MaybeEat(Comma)) {
                FailExpression = Expression.ParsePreGlobalExpr(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public AssertStatement(
            Expression condition,
            Expression failExpression
        ) {
            Condition      = condition;
            FailExpression = failExpression;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("assert ", Condition);
            if (FailExpression != null) {
                c.Write(", ", FailExpression);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("Debug.Assert(", Condition);
            if (FailExpression != null) {
                c.Write(", ", FailExpression);
            }

            c.Write(");");
        }
    }
}