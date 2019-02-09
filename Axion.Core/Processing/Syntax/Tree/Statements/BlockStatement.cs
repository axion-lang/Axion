using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class BlockStatement : Statement {
        private Statement[] statements;

        internal BlockStatement(Statement[] statements) {
            if (statements.Length == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(statements));
            }
            Statements = statements;
            MarkPosition(statements[0], statements[statements.Length - 1]);
        }

        internal BlockStatement(Statement[] statements, Position start, Position end) {
            Statements = statements;
            MarkPosition(start, end);
        }

        [JsonProperty]
        public Statement[] Statements {
            get => statements;
            set {
                statements = value;
                foreach (Statement expr in statements) {
                    expr.Parent = this;
                }
            }
        }
    }
}