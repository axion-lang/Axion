using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class AssignmentExpression : Expression {
        private Expression[] left;

        /// <summary>
        ///     len = 1 for x = 4;
        ///     len = 3 for x = y = z = 4
        /// </summary>
        [JsonProperty]
        internal Expression[] Left {
            get => left;
            set {
                left = value;
                foreach (Expression expr in left) {
                    expr.Parent = this;
                }
            }
        }

        private Expression right;

        [JsonProperty]
        internal Expression Right {
            get => right;
            set {
                value.Parent = this;
                right        = value;
            }
        }

        public AssignmentExpression(Expression left, Expression right) {
            Left = left != null
                ? new[] {
                    left
                }
                : throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public AssignmentExpression(Expression[] left, Expression right) {
            Left  = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c.AppendJoin(" = ", Left);
            return c + " = " + Right;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c.AppendJoin(" = ", Left);
            return c + " = " + Right;
        }
    }
}