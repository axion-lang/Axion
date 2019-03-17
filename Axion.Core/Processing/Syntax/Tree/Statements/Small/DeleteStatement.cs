using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class DeleteStatement : Statement {
        private Expression[] expressions;

        public Expression[] Expressions {
            get => expressions;
            set {
                expressions = value;
                foreach (Expression expr in expressions) {
                    expr.Parent = this;
                }
            }
        }

        internal DeleteStatement(
            Token        startToken,
            Expression[] expressions
        ) : base(startToken) {
            Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
            if (expressions.Length == 0) {
                throw new ArgumentException(
                    "Value cannot be an empty collection.",
                    nameof(expressions)
                );
            }

            MarkEnd(Expressions[Expressions.Length - 1]);
        }
    }
}