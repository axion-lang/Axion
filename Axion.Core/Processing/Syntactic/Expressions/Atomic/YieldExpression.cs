using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         yield_expr:
    ///             'yield' ['from' test | test_list]
    ///     </c>
    /// </summary>
    public class YieldExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal bool IsYieldFrom { get; }

        internal YieldExpression(
            Expression value,
            bool       isYieldFrom
        ) {
            Value       = value ?? new ConstantExpression(KeywordNil);
            IsYieldFrom = isYieldFrom;
        }

        internal YieldExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(KeywordYield);

            // Parse expr list after yield. This can be:
            // 1) a single expr
            // 2) multiple expressions, in which case it's wrapped in a tuple.
            if (MaybeEat(KeywordFrom)) {
                Value       = ParseInfixExpr(this);
                IsYieldFrom = true;
            }
            else {
                Value = ParseMultiple(this, expectedTypes: Spec.InfixExprs);
            }

            MarkEnd();
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("yield ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("yield ", Value);
        }
    }
}