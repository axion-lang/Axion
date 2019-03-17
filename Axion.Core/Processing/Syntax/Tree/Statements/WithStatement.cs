using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WithStatement : Statement {
        private WithStatementItem item;

        public WithStatementItem Item {
            get => item;
            set {
                value.Parent = this;
                item         = value;
            }
        }

        private Statement block;

        public Statement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        internal WithStatement(
            Token             startToken,
            WithStatementItem item,
            Statement         block
        ) : base(startToken) {
            Item  = item ?? throw new ArgumentNullException(nameof(item));
            Block = block ?? throw new ArgumentNullException(nameof(block));

            MarkEnd(Block);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + "with " + item + " " + Block;
        }
    }

    public class WithStatementItem : Statement {
        private Expression contextManager;

        public Expression ContextManager {
            get => contextManager;
            set {
                value.Parent   = this;
                contextManager = value;
            }
        }

        private Expression name;

        public Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        public WithStatementItem(
            Token      startToken,
            Expression contextManager,
            Expression name
        ) : base(startToken) {
            ContextManager =
                contextManager ?? throw new ArgumentNullException(nameof(contextManager));
            Name = name ?? throw new ArgumentNullException(nameof(name));

            MarkEnd(Name);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + ContextManager + " as " + Name;
        }
    }
}