using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         return_stmt:
    ///             'return' [preglobal_list]
    ///     </c>
    /// </summary>
    public class ReturnStatement : Statement {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ReturnStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(TokenType.KeywordReturn);

            if (Ast.CurrentFunction == null) {
                Unit.Blame(BlameType.MisplacedReturn, Token);
            }

            if (!Peek.Is(Spec.NeverExprStartTypes)) {
                Value = Expression.ParseMultiple(parent, expectedTypes: Spec.PreGlobalExprs);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public ReturnStatement(Expression value) {
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