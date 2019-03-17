using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class YieldExpression : Expression {
        internal bool       IsYieldFrom { get; }
        private  Expression expression;

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
            Token      start,
            Token      end
        ) : base(start, end) {
            Expression  = expression;
            IsYieldFrom = isYieldFrom;
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + "yield " + Expression;
        }
    }
}