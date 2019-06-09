using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         return_expr:
    ///             'return' [preglobal_list]
    ///     </c>
    /// </summary>
    public class ReturnExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ReturnExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.KeywordReturn);

            if (ParentBlock.CurrentFunction == null) {
                Unit.Blame(BlameType.MisplacedReturn, Token);
            }

            if (!Peek.Is(Spec.NeverExprStartTypes)) {
                Value = ParseMultiple(parent, expectedTypes: Spec.InfixExprs);
            }

            MarkEnd();
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public ReturnExpression(Expression value) {
            Value = value;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("return ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("return ", Value, ";");
        }
    }
}