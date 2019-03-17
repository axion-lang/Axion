using System;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class BlockStatement : Statement {
        private Statement[] statements;

        public Statement[] Statements {
            get => statements;
            set {
                statements = value;
                foreach (Statement expr in statements) {
                    expr.Parent = this;
                }
            }
        }

        internal BlockStatement(params Statement[] statements) {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
            if (statements.Length != 0) {
                MarkPosition(statements[0], statements[statements.Length - 1]);
            }
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c += "{ ";
            c.AppendJoin("; ", Statements);
            return c + "; }";
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c += "{ ";
            c.AppendJoin("", Statements);
            return c + " }";
        }
    }
}