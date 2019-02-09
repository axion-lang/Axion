using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class DeleteStatement : Statement {
        private Expression[] expressions;

        internal DeleteStatement(Expression[] expressions, SpannedRegion start) {
            Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
            if (expressions.Length == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(expressions));
            }

            MarkStart(start);
            MarkEnd(Expressions[Expressions.Length - 1]);
        }

        [JsonProperty]
        internal Expression[] Expressions {
            get => expressions;
            set {
                expressions = value;
                foreach (Expression expr in expressions) {
                    expr.Parent = this;
                }
            }
        }
    }
}