using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Comprehensions {
    public class ComprehensionIterator : Expression { }

    public class ForComprehension : ComprehensionIterator {
        public Expression Left { get; }
        public Expression List { get; }

        public ForComprehension(Expression left, Expression list) {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            List = list ?? throw new ArgumentNullException(nameof(list));
        }

        public ForComprehension(SpannedRegion start, Expression left, Expression list) : this(
            left,
            list
        ) {
            MarkPosition(start, list);
        }
    }

    public class IfComprehension : ComprehensionIterator {
        private Expression condition;

        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        public IfComprehension(Expression condition) {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public IfComprehension(SpannedRegion start, Expression condition) : this(condition) {
            MarkPosition(start, Condition);
        }
    }
}