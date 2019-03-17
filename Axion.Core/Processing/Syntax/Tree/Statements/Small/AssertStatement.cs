using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class AssertStatement : Statement {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        private Expression failExpression;

        public Expression FailExpression {
            get => failExpression;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                failExpression = value;
            }
        }

        internal AssertStatement(
            Token      startToken,
            Expression condition,
            Expression failExpression
        ) : base(startToken) {
            Condition      = condition ?? throw new ArgumentNullException(nameof(condition));
            FailExpression = failExpression;

            MarkEnd(failExpression ?? condition);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + "assert " + Condition;
            if (FailExpression != null) {
                c = c + ", " + FailExpression;
            }

            return c;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c = c + "Debug.Assert(" + Condition;
            if (FailExpression != null) {
                c = c + ", " + FailExpression;
            }

            return c + ");";
        }
    }
}