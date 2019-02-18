using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class RaiseStatement : Statement {
        private Expression exception;

        internal RaiseStatement(Expression exception, Expression cause, SpannedRegion start) {
            Exception = exception;
            Cause     = cause;

            MarkStart(start);
            MarkEnd(cause ?? exception ?? start);
        }

        [JsonProperty]
        public Expression Cause { get; }

        [JsonProperty]
        internal Expression Exception {
            get => exception;
            set {
                value.Parent = this;
                exception    = value;
            }
        }
    }
}