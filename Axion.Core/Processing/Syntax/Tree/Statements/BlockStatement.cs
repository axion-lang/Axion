using System;
using System.CodeDom;
using System.Linq;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class BlockStatement : Statement {
        private Statement[] statements;

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

        internal BlockStatement(Statement[] statements) {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
            if (statements.Length != 0) {
                MarkPosition(statements[0], statements[statements.Length - 1]);
            }
        }

        internal override CodeObject[] ToCSharpArray() {
            return Statements.Select(s => s.ToCSharp()).ToArray();
        }
    }
}