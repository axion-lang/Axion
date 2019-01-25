using System.Collections.Generic;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ListExpression : Expression {
        private List<Expression> expressions;

        [JsonProperty]
        internal List<Expression> Expressions {
            get => expressions;
            set {
                expressions = value;
                foreach (Expression expr in expressions) {
                    expr.Parent = this;
                }
            }
        }

        internal ListExpression(Span region, List<Expression> expressions) {
            Expressions = expressions;
            Span        = region;
        }
    }
}