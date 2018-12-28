using System;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class DeleteStatement : Statement {
        [JsonProperty]
        internal Expression[] Expressions { get; }

        internal DeleteStatement(List<Expression> expressions, SpannedRegion start) {
            if (expressions.Count == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(expressions));
            }

            Expressions = expressions.ToArray();

            MarkStart(start);
            MarkEnd(Expressions[Expressions.Length - 1]);
        }
    }
}