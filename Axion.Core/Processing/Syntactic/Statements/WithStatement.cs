using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         with_stmt ::=
    ///             'with' with_item {',' with_item} block
    ///         with_item ::=
    ///             test ['as' name]
    ///     </c>
    /// </summary>
    public class WithStatement : Statement {
        private WithStatementItem item;

        [NotNull]
        public WithStatementItem Item {
            get => item;
            set => SetNode(ref item, value);
        }

        private Statement block;

        [NotNull]
        public Statement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public WithStatement(
            [NotNull] Token             startToken,
            [NotNull] WithStatementItem item,
            [NotNull] Statement         block
        ) {
            Item  = item;
            Block = block;

            MarkPosition(startToken, Block);
        }

        internal WithStatement(SyntaxTreeNode parent) {
            Parent = parent;
            Token start = StartNode(TokenType.KeywordWith);

            // TODO: add 'with' expression like in Kotlin
            var items = new List<WithStatementItem>();
            do {
                items.Add(new WithStatementItem(this));
            } while (MaybeEat(TokenType.Comma));

            Block = new BlockStatement(this);
            if (items.Count > 1) {
                // nest multiple 'with' items
                for (int i = items.Count - 1; i > 0; i--) {
                    Block = new WithStatement(start, items[i], Block);
                }
            }

            Item = items[0];

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "with " + Item + " " + Block;
        }

        // TODO: implement 'using' in c#
    }

    public class WithStatementItem : Statement {
        private Expression contextManager;

        [NotNull]
        public Expression ContextManager {
            get => contextManager;
            set => SetNode(ref contextManager, value);
        }

        private Expression name;

        [NotNull]
        public Expression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        public WithStatementItem([NotNull] Expression contextManager, [NotNull] Expression name) {
            ContextManager = contextManager;
            Name           = name;

            MarkPosition(ContextManager, Name);
        }

        internal WithStatementItem(SyntaxTreeNode parent) {
            Parent = parent;
            MarkStart(Token);

            ContextManager = Expression.ParseTestExpr(this);
            if (MaybeEat(TokenType.OpAs)) {
                Name = new NameExpression(this, true);
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + ContextManager + " as " + Name;
        }
    }
}