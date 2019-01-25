using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class BodyStatement : Statement {
        private Statement[] statements;

        [JsonProperty]
        internal Statement[] Statements {
            get => statements;
            set {
                statements = value;
                foreach (Statement expr in statements) {
                    expr.Parent = this;
                }
            }
        }

        internal BodyStatement(Statement[] statements) {
            if (statements.Length == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(statements));
            }
            Statements = statements;

            MarkStart(statements[0]);
            MarkEnd(statements[statements.Length - 1].Span.End);
        }
    }
}