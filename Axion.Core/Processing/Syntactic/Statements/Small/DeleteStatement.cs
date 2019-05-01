using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     <c>
    ///         delete_stmt:
    ///             'delete' expr_list
    ///     </c>
    ///     <para />
    ///     For error reporting reasons we allow any
    ///     expr and then report the bad delete node when it fails.
    ///     This is the reason we don't call ParseTargetList.
    /// </summary>
    public class DeleteStatement : Statement {
        private Expression value;

        public Expression Value {
            get => value;
            set => SetNode(ref this.value, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal DeleteStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(TokenType.KeywordDelete);
            Value = Expression.ParseMultiple(this, expectedTypes: Spec.AssignableExprs);
            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public DeleteStatement(Expression value) {
            Value = value;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("delete ", Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}