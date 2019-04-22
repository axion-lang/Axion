using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         assert_stmt:
    ///             'assert' test [',' test]
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

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="AssertStatement"/> from tokens.
        /// </summary>
        internal AssertStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordAssert);

            Condition = Expression.ParseTestExpr(this);
            if (MaybeEat(TokenType.Comma)) {
                FailExpression = Expression.ParseTestExpr(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="AssertStatement"/> without position in source.
        /// </summary>
        public AssertStatement(
            Expression condition,
            Expression failExpression
        ) {
            Condition      = condition;
            FailExpression = failExpression;
        }

        #endregion

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("assert ", Condition);
            if (FailExpression != null) {
                c.Write(", ", FailExpression);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("Debug.Assert(", Condition);
            if (FailExpression != null) {
                c.Write(", ", FailExpression);
            }

            c.Write(");");
        }

        #endregion
    }
}