using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions {
    public class IfComprehension : ComprehensionIterator {
        private Expression condition;

        public IfComprehension(SpannedRegion start, Expression condition) {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));

            MarkStart(start);
            MarkEnd(Condition);
        }

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }
    }
}