using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         with_stmt:
    ///             'with' with_item {',' with_item} block
    ///     </c>
    /// </summary>
    public class WithStatement : Statement {
        #region Properties

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

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="WithStatement"/> from tokens.
        /// </summary>
        internal WithStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordWith);

            // TODO: add 'with' expression like in Kotlin
            var items = new List<WithStatementItem>();
            do {
                items.Add(new WithStatementItem(this));
            } while (MaybeEat(TokenType.Comma));

            Block = new BlockStatement(this);
            if (items.Count > 1) {
                // nest multiple 'with' items
                for (int i = items.Count - 1; i > 0; i--) {
                    Block = new BlockStatement(this, new WithStatement(items[i], Block));
                }
            }

            Item = items[0];

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="WithStatement"/> without position in source.
        /// </summary>
        public WithStatement(WithStatementItem item, BlockStatement block) {
            Item  = item;
            Block = block;
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("with ", Item, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("using (", item, ") ", Block);
        }

        #endregion
    }

    /// <summary>
    ///     <c>
    ///         with_item:
    ///             primary ['as' name]
    ///     </c>
    /// </summary>
    public class WithStatementItem : Statement {
        #region Properties

        private Expression contextManager;

        public Expression ContextManager {
            get => contextManager;
            set => SetNode(ref contextManager, value);
        }

        private Expression name;

        public Expression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="WithStatementItem"/> from tokens.
        /// </summary>
        internal WithStatementItem(SyntaxTreeNode parent) : base(parent) {
            MarkStart(Token);

            ContextManager = Expression.ParseExtended(this);
            if (MaybeEat(TokenType.KeywordAs)) {
                Name = new NameExpression(this, true);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="WithStatement"/> without position in source.
        /// </summary>
        public WithStatementItem(Expression contextManager, Expression name) {
            ContextManager = contextManager;
            Name           = name;
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(ContextManager, " as ", Name);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Name, " = ", ContextManager);
        }

        #endregion
    }
}