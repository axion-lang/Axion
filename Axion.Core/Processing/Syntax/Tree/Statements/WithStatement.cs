using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class WithStatement : Statement {
        [JsonProperty]
        public WithStatementItem Item { get; }

        [JsonProperty]
        public Statement Body { get; }

        internal WithStatement(WithStatementItem item, Statement body, SpannedRegion start) {
            Item = item;
            Body = body;
            MarkStart(start);
            MarkEnd(Body);
        }
    }

    public struct WithStatementItem {
        public readonly Position   Start;
        public readonly Expression ContextManager;
        public readonly Expression Variable;

        public WithStatementItem(Position start, Expression contextManager, Expression variable) {
            Start          = start;
            ContextManager = contextManager;
            Variable       = variable;
        }
    }
}