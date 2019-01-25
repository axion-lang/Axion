using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class TupleExpression : Expression {
        internal bool Expandable;

        private Expression[] expressions;

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

        internal TupleExpression(bool expandable, Expression[] expressions) {
            Expandable  = expandable;
            Expressions = expressions;
            if (expressions.Length > 0) {
                MarkPosition(expressions[0].Span.Start, expressions[expressions.Length - 1].Span.End);
            }
        }
    }
}