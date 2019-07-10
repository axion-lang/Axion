using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         return_expr:
    ///             'return' [preglobal_list];
    ///     </c>
    /// </summary>
    public class ReturnExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal ReturnExpression(Expression parent) {
            Construct(parent, () => {
                Eat(KeywordReturn);

                if (ParentBlock.CurrentFunction == null) {
                    Unit.Blame(BlameType.MisplacedReturn, Token);
                }

                if (!Peek.Is(Spec.NeverExprStartTypes)) {
                    Value = ParseMultiple(expectedTypes: Spec.InfixExprs);
                }
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("return ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("return ", Value, ";");
        }
    }
}