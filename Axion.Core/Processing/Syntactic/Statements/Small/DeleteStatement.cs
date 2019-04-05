using System;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         delete_stmt ::=
    ///             'delete' expr_list
    ///     </c>
    ///     <para />
    ///     For error reporting reasons we allow any
    ///     expr and then report the bad delete node when it fails.
    ///     This is the reason we don't call ParseTargetList.
    /// </summary>
    public class DeleteStatement : Statement {
        private TestList values;

        [NotNull]
        public TestList Values {
            get => values;
            set => SetNode(ref values, value);
        }

        public DeleteStatement([NotNull] TestList expressions) {
            Values = expressions;
            if (Values.Count == 0) {
                throw new ArgumentException(
                    "Value cannot be an empty collection.",
                    nameof(expressions)
                );
            }

            MarkEnd(Values[Values.Count - 1]);
        }

        internal DeleteStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordDelete);

            Values = new TestList(this, out bool _);
            foreach (Expression expr in Values.Expressions) {
                if (expr.CannotDeleteReason != null) {
                    Unit.Blame(BlameType.InvalidExpressionToDelete, expr);
                }
            }

            MarkEnd(Token);
        }
    }
}