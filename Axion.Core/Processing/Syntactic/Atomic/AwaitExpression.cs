using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         await_expr:
    ///             'await' expr_list;
    ///     </c>
    /// </summary>
    public class AwaitExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public override TypeName ValueType => Value.ValueType;

        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal AwaitExpression(Expression parent) {
            Construct(parent, () => {
                // TODO: add 'in async context' check
                Eat(KeywordAwait);
                Value = ParseMultiple(parent, expectedTypes: Spec.InfixExprs);
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("await ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("await ", Value);
        }
    }
}