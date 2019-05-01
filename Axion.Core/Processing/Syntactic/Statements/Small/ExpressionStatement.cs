using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    public class ExpressionStatement : Statement {
        private Expression expression;

        public Expression Expression {
            get => expression;
            set => SetNode(ref expression, value);
        }

        public ExpressionStatement(Expression expression) {
            Parent     = expression.Parent;
            Expression = expression;
            Expression.CheckType(expression, Spec.StatementExprs);
            MarkPosition(expression);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Expression);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Expression, ";");
        }
    }
}