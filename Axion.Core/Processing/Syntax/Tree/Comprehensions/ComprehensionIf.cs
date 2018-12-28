using System;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Comprehensions {
    public class ComprehensionIf : ComprehensionIterator {
        public Expression Test { get; }

        public ComprehensionIf(SpannedRegion start, Expression test) {
            Test = test ?? throw new ArgumentNullException(nameof(test));

            MarkStart(start);
            MarkEnd(Test);
        }
    }
}