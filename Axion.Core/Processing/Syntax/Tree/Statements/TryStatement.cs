using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class TryStatement : Statement {
        private Statement body;

        [JsonProperty]
        internal Statement Body {
            get => body;
            set {
                value.Parent = this;
                body         = value;
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

        private Statement elseBody;

        [JsonProperty]
        internal Statement ElseBody {
            get => elseBody;
            set {
                value.Parent = this;
                elseBody     = value;
            }
        }

        private Statement anywayBody;

        [JsonProperty]
        internal Statement AnywayBody {
            get => anywayBody;
            set {
                value.Parent = this;
                anywayBody   = value;
            }
        }

        internal TryStatement(
            Statement             body,
            TryStatementHandler[] handlers,
            Statement             elseBlock,
            Statement             anywayBlock,
            SpannedRegion         start
        ) {
            Body       = body;
            Handlers   = handlers ?? new TryStatementHandler[0];
            ElseBody   = elseBlock;
            AnywayBody = anywayBlock;

            MarkStart(start);
            MarkEnd(AnywayBody ?? ElseBody ?? (Handlers.Length > 0 ? Handlers[Handlers.Length - 1] : (TreeNode) body));
        }
    }

    public class TryStatementHandler : TreeNode {
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

        private Statement body;

        [JsonProperty]
        internal Statement Body {
            get => body;
            set {
                value.Parent = this;
                body         = value;
            }
        }

        public TryStatementHandler(Expression errorType, Expression target, Statement body, Position start) {
            ErrorType = errorType;
            Name      = target;
            Body      = body;

            MarkStart(start);
            MarkEnd(Body);
        }
    }
}