using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class TryStatement : Statement {
        private BlockStatement block;

        [JsonProperty]
        internal BlockStatement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        private TryStatementHandler[] handlers;

        [JsonProperty]
        internal TryStatementHandler[] Handlers {
            get => handlers;
            set {
                handlers = value;
                foreach (TryStatementHandler expr in handlers) {
                    expr.Parent = this;
                }
            }
        }

        private BlockStatement elseBlock;

        [JsonProperty]
        internal BlockStatement ElseBlock {
            get => elseBlock;
            set {
                value.Parent = this;
                elseBlock    = value;
            }
        }

        private BlockStatement anywayBlock;

        [JsonProperty]
        internal BlockStatement AnywayBlock {
            get => anywayBlock;
            set {
                value.Parent = this;
                anywayBlock  = value;
            }
        }

        internal TryStatement(
            BlockStatement        block,
            TryStatementHandler[] handlers,
            BlockStatement        elseBlock,
            BlockStatement        anywayBlock,
            SpannedRegion         start
        ) {
            Block       = block;
            Handlers    = handlers ?? new TryStatementHandler[0];
            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;

            MarkStart(start);
            MarkEnd(
                AnywayBlock
             ?? ElseBlock
             ?? (Handlers.Length > 0 ? Handlers[Handlers.Length - 1] : (SyntaxTreeNode) block)
            );
        }
    }

    public class TryStatementHandler : SyntaxTreeNode {
        private Expression errorType;

        [JsonProperty]
        internal Expression ErrorType {
            get => errorType;
            set {
                value.Parent = this;
                errorType    = value;
            }
        }

        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private BlockStatement block;

        [JsonProperty]
        internal BlockStatement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

        public TryStatementHandler(
            Expression     errorType,
            Expression     target,
            BlockStatement block,
            Position       start
        ) {
            ErrorType = errorType;
            Name      = target;
            Block     = block;

            MarkStart(start);
            MarkEnd(Block);
        }
    }
}