using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    public class ExpressionStatement : Statement {
        private Expression expression;

        [NotNull]
        public Expression Expression {
            get => expression;
            set => SetNode(ref expression, value);
        }

        public ExpressionStatement([NotNull] Expression expression) {
            Expression = expression;
            MarkPosition(expression);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Expression;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c += Expression;
            return c + ";";
        }
    }
}