using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class RaiseStatement : Statement {
        private Expression exception;

        [JsonProperty]
        internal Expression Exception {
            get => exception;
            set {
                value.Parent = this;
                exception    = value;
            }
        }

        [JsonProperty]
        public Expression Cause { get; }

        internal RaiseStatement(Expression exception, Expression cause, SpannedRegion start) {
            Exception = exception;
            Cause     = cause;

            MarkStart(start);
            MarkEnd(cause ?? exception ?? start);
        }
    }
}