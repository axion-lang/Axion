using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         return_stmt ::=
    ///             'return' [test_list]
    ///     </c>
    /// </summary>
    public class ReturnStatement : Statement {
        private Expression val;

        [NotNull]
        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public ReturnStatement([NotNull] Expression expression) {
            Value = expression;
        }

        internal ReturnStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordReturn);

            if (Ast.CurrentFunction == null) {
                Unit.Blame(BlameType.MisplacedReturn, Token);
            }

            if (!PeekIs(Spec.NeverTestTypes)) {
                Value = Expression.SingleOrTuple(this);
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "return " + Value;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + "return " + Value + ";";
        }
    }
}