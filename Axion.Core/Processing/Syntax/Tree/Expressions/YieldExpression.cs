namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class YieldExpression : Expression {
        internal Expression Expression  { get; }
        internal bool       IsYieldFrom { get; }

        internal YieldExpression(Expression expression, bool isYieldFrom = false) {
            Expression  = expression;
            IsYieldFrom = isYieldFrom;
            MarkPosition(expression);
        }

        internal YieldExpression(Expression expression, bool isYieldFrom, Position start, Position end)
            : base(start, end) {
            Expression  = expression;
            IsYieldFrom = isYieldFrom;
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return "yield " + Expression;
        }
    }
}