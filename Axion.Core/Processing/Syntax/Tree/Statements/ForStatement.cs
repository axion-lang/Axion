using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForStatement : Statement {
        [JsonProperty]
        public Expression Left { get; }

        private Expression list;

        [JsonProperty]
        internal Expression List {
            get => list;
            set {
                value.Parent = this;
                list         = value;
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

        private Statement noBreakBody;

        [JsonProperty]
        internal Statement NoBreakBody {
            get => noBreakBody;
            set {
                value.Parent = this;
                noBreakBody  = value;
            }
        }

        public ForStatement(Expression left, Expression list, Statement body, Statement noBreakBody, SpannedRegion start) {
            Left        = left;
            List        = list ?? throw new ArgumentNullException(nameof(list));
            Body        = body;
            NoBreakBody = noBreakBody;

            MarkStart(start);
            MarkEnd(NoBreakBody ?? (SpannedRegion) Body ?? List);
        }
    }
}