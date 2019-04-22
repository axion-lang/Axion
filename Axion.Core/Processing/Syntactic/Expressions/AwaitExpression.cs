using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         await_expr:
    ///             'await' expr
    ///     </c>
    /// </summary>
    public class AwaitExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal override TypeName ValueType => Value.ValueType;

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="AwaitExpression"/> from tokens.
        /// </summary>
        internal AwaitExpression(SyntaxTreeNode parent) : base(parent) {
            // TODO: add 'in async context' check            
            MarkStart(TokenType.KeywordAwait);
            Value = ParseExpression(parent, expectedTypes: Spec.TestExprs);
            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="AwaitExpression"/> without position in source.
        /// </summary>
        public AwaitExpression(Expression value) {
            Value = value;
        }

        #endregion

        #region Transpilers

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("await ", Value);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("await ", Value);
        }

        #endregion
    }
}