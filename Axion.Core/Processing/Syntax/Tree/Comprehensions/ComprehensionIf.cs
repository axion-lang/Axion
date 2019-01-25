using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Comprehensions {
    public class ComprehensionIf : ComprehensionIterator {
        private Expression condition;

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        public ComprehensionIf(SpannedRegion start, Expression condition) {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));

            MarkStart(start);
            MarkEnd(Condition);
        }
    }
}