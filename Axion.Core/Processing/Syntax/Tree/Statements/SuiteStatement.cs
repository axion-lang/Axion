using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class SuiteStatement : Statement {
        [JsonProperty]
        public Statement[] Statements { get; }

        internal SuiteStatement(List<Statement> statements) {
            if (statements.Count == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(statements));
            }
            Statements = statements.ToArray();

            MarkStart(statements[0]);
            MarkEnd(statements[statements.Count - 1].Span.End);
        }
    }
}