using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;

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
            MarkPosition(expression);
        }

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(Expression);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write(Expression, ";");
        }

        #endregion
    }
}