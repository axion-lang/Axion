using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForStatement : Statement, IAsyncStatement {
        [JsonProperty]
        public Expression Left { get; }

        [JsonProperty]
        public Statement Body { get; set; }

        [JsonProperty]
        public Expression List { get; set; }

        [JsonProperty]
        public Statement NoBreakBody { get; }

        [JsonProperty]
        public bool IsAsync { get; set; }

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