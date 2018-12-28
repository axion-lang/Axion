using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class TryStatement : Statement {
        [JsonProperty]
        public Statement Body { get; }

        [JsonProperty]
        public TryStatementHandler[] Handlers { get; }

        [JsonProperty]
        public Statement ElseBlock { get; }

        [JsonProperty]
        public Statement AnywayBlock { get; }

        internal TryStatement(
            Statement                 body,
            List<TryStatementHandler> handlers,
            Statement                 elseBlock,
            Statement                 anywayBlock,
            SpannedRegion             start
        ) {
            Body        = body;
            Handlers    = handlers?.ToArray() ?? new TryStatementHandler[0];
            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;

            MarkStart(start);
            if (AnywayBlock != null) {
                MarkEnd(AnywayBlock);
            }
            else if (ElseBlock != null) {
                MarkEnd(ElseBlock);
            }
            else if (Handlers != null) {
                MarkEnd(Handlers[Handlers.Length - 1]);
            }
            else {
                MarkEnd(body);
            }
        }
    }

    public class TryStatementHandler : SpannedRegion {
        [JsonProperty]
        public Expression Test { get; }

        [JsonProperty]
        public Expression Target { get; }

        [JsonProperty]
        public Statement Body { get; }

        public TryStatementHandler(Expression test, Expression target, Statement body, Position start) {
            Test   = test;
            Target = target;
            Body   = body;

            MarkStart(start);
            MarkEnd(Body);
        }
    }
}