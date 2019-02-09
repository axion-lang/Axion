using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class TryStatement : Statement {
        private Statement block;

        private TryStatementHandler[] handlers;

        private Statement elseBlock;

        private Statement anywayBlock;

        internal TryStatement(
            Statement             block,
            TryStatementHandler[] handlers,
            Statement             elseBlock,
            Statement             anywayBlock,
            SpannedRegion         start
        ) {
            Block       = block;
            Handlers    = handlers ?? new TryStatementHandler[0];
            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;

            MarkStart(start);
            MarkEnd(
                AnywayBlock ?? ElseBlock ?? (Handlers.Length > 0 ? Handlers[Handlers.Length - 1] : (TreeNode) block)
            );
        }

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }

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

        [JsonProperty]
        internal Statement ElseBlock {
            get => elseBlock;
            set {
                value.Parent = this;
                elseBlock    = value;
            }
        }

        [JsonProperty]
        internal Statement AnywayBlock {
            get => anywayBlock;
            set {
                value.Parent = this;
                anywayBlock  = value;
            }
        }
    }

    public class TryStatementHandler : TreeNode {
        private Expression errorType;

        private Expression name;

        private Statement block;

        public TryStatementHandler(Expression errorType, Expression target, Statement block, Position start) {
            ErrorType = errorType;
            Name      = target;
            Block     = block;

            MarkStart(start);
            MarkEnd(Block);
        }

        [JsonProperty]
        internal Expression ErrorType {
            get => errorType;
            set {
                value.Parent = this;
                errorType    = value;
            }
        }

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                value.Parent = this;
                block        = value;
            }
        }
    }
}