using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
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
        ///     Constructs expression from tokens.
        /// </summary>
        internal AwaitExpression(SyntaxTreeNode parent) : base(parent) {
            // TODO: add 'in async context' check            
            EatStartMark(KeywordAwait);
            Value = ParseMultiple(parent, expectedTypes: Spec.PreGlobalExprs);
            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public AwaitExpression(Expression value) {
            Value = value;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("await ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("await ", Value);
        }
    }
}