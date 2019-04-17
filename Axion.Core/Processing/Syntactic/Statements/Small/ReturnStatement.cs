using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         return_stmt:
    ///             'return' [list_test]
    ///     </c>
    /// </summary>
    public class ReturnStatement : Statement {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        /// <summary>
        ///     Constructs new <see cref="ReturnStatement"/> from tokens.
        /// </summary>
        internal ReturnStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordReturn);

            if (Ast.CurrentFunction == null) {
                Unit.Blame(BlameType.MisplacedReturn, Token);
            }

            if (!PeekIs(Spec.NeverTestTypes)) {
                Value = Expression.ParseMultiple(parent, expectedTypes: Spec.TestExprs);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="ReturnStatement"/> without position in source.
        /// </summary>
        public ReturnStatement(Expression value) {
            Value = value;
        }

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("return ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("return ", Value, ";");
        }

        #endregion
    }
}