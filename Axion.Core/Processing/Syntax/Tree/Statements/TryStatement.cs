using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class TryStatement : Statement {
        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        private TryStatementHandler[] handlers;

        public TryStatementHandler[] Handlers {
            get => handlers;
            set {
                handlers = value;
                foreach (TryStatementHandler expr in handlers) {
                    expr.Parent = this;
                }
            }
        }

        private BlockStatement elseBlock;

        public BlockStatement ElseBlock {
            get => elseBlock;
            set {
                value.Parent = this;
                elseBlock    = value;
            }
        }

        private BlockStatement anywayBlock;

        public BlockStatement AnywayBlock {
            get => anywayBlock;
            set {
                value.Parent = this;
                anywayBlock  = value;
            }
        }

        internal TryStatement(
            Token                 startToken,
            BlockStatement        block,
            TryStatementHandler[] handlers,
            BlockStatement        elseBlock,
            BlockStatement        anywayBlock
        ) : base(startToken) {
            Block       = block;
            Handlers    = handlers ?? new TryStatementHandler[0];
            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;

            MarkEnd(
                AnywayBlock
                ?? ElseBlock
                ?? (Handlers.Length > 0
                    ? Handlers[Handlers.Length - 1]
                    : (SyntaxTreeNode) block)
            );
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + "try " + Block;
            if (Handlers.Length > 0) {
                c.AppendJoin(" ", Handlers);
            }

            if (AnywayBlock != null) {
                c = c + " anyway " + AnywayBlock;
            }

            return c;
        }
    }

    public class TryStatementHandler : Statement {
        private TypeName errorType;

        public TypeName ErrorType {
            get => errorType;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                errorType = value;
            }
        }

        private Expression name;

        public Expression Name {
            get => name;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                name = value;
            }
        }

        private Expression condition;

        public Expression Condition {
            get => condition;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                condition = value;
            }
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        public TryStatementHandler(
            Token          startToken,
            TypeName       errorType,
            Expression     errorName,
            Expression     condition,
            BlockStatement block
        ) : base(startToken) {
            ErrorType = errorType;
            Name      = errorName;
            Condition = condition;
            Block     = block ?? throw new ArgumentNullException(nameof(block));

            MarkEnd(Block);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c += "catch";
            if (ErrorType != null) {
                c = c + " " + ErrorType;
            }

            if (Name != null) {
                c = c + " as " + Name;
            }

            if (Condition != null) {
                c = c + " when " + Condition;
            }

            return c + Block;
        }
    }
}