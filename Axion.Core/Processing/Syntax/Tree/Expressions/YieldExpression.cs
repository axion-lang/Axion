using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class YieldExpression : Expression {
        internal bool IsYieldFrom { get; }

        private Expression expression;

        [JsonProperty]
        internal Expression Expression {
            get => expression;
            set {
                value.Parent = this;
                expression   = value;
            }
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        internal YieldExpression(
            Expression expression,
            bool       isYieldFrom,
            Position   start,
            Position   end
        ) : base(start, end) {
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