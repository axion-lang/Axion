using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         with_stmt:
    ///             'with' with_item {',' with_item} block
    ///     </c>
    /// </summary>
    public class WithStatement : Statement {
        private WithStatementItem item;

        public WithStatementItem Item {
            get => item;
            set => SetNode(ref item, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal WithStatement(AstNode parent) : base(parent) {
            MarkStartAndEat(KeywordWith);

            var items = new List<WithStatementItem>();
            do {
                items.Add(new WithStatementItem(this));
            } while (MaybeEat(Comma));

            Block = new BlockStatement(this);
            if (items.Count > 1) {
                // nest multiple 'with' items
                for (int i = items.Count - 1; i > 0; i--) {
                    Block = new BlockStatement(this, new WithStatement(items[i], Block));
                }
            }

            Item = items[0];

            MarkEnd();
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public WithStatement(WithStatementItem item, BlockStatement block) {
            Item  = item;
            Block = block;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("with ", Item, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("using (", item, ") ", Block);
        }
    }

    /// <summary>
    ///     <c>
    ///         with_item:
    ///             atom ['as' name]
    ///     </c>
    /// </summary>
    public class WithStatementItem : Statement {
        private Expression contextManager;

        public Expression ContextManager {
            get => contextManager;
            set => SetNode(ref contextManager, value);
        }

        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal WithStatementItem(AstNode parent) : base(parent) {
            MarkStart();

            ContextManager = Expression.ParseSuffixExpr(this);
            if (MaybeEat(KeywordAs)) {
                Name = new SimpleNameExpression(this);
            }

            MarkEnd();
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public WithStatementItem(Expression contextManager, SimpleNameExpression name) {
            ContextManager = contextManager;
            Name           = name;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(ContextManager, " as ", Name);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("var ", Name, " = ", ContextManager);
        }
    }
}