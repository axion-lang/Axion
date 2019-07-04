using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
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

        internal bool IsYieldFrom => Value != null;

        internal YieldExpression(Expression parent) {
            Construct(parent, () => {
                Eat(KeywordYield);

                // Parse expr list after yield. This can be:
                // 1) a single expr
                // 2) multiple expressions, in which case it's wrapped in a tuple.
                if (MaybeEat(KeywordFrom)) {
                    Value = ParseInfixExpr(this);
                }
                else {
                    Value = ParseMultiple(this, expectedTypes: Spec.InfixExprs);
                }
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("yield ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("yield ", Value);
        }
    }
}