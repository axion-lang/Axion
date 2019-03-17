using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class RaiseStatement : Statement {
        private Expression exception;

        public Expression Exception {
            get => exception;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                exception = value;
            }
        }

        private Expression cause;

        public Expression Cause {
            get => cause;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                cause = value;
            }
        }

        internal RaiseStatement(
            Token      startToken,
            Expression exception,
            Expression cause
        ) : base(startToken) {
            Exception = exception;
            Cause     = cause;

            MarkEnd(cause ?? (SpannedRegion) exception ?? startToken);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c += "raise";
            if (Exception != null) {
                c = c + " " + Exception;
            }

            if (Cause != null) {
                c = c + " " + Cause;
            }

            return c;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c += "throw new Exception(";
            if (Exception is ConstantExpression constant) {
                c = c + constant + ".ToString()";
            }

            return c + ");";
        }
    }
}