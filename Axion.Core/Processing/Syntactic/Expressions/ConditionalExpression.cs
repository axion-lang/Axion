using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         cond_expr ::=
    ///             expr ('if' | 'unless') priority_expr ['else' expr]
    ///     </c>
    /// </summary>
    public class ConditionalExpression : Expression {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private Expression trueExpression;

        [NotNull]
        public Expression TrueExpression {
            get => trueExpression;
            set => SetNode(ref trueExpression, value);
        }

        private Expression falseExpression;

        public Expression FalseExpression {
            get => falseExpression;
            set => SetNode(ref falseExpression, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public ConditionalExpression(
            Expression           condition,
            [NotNull] Expression trueExpression,
            Expression           falseExpression
        ) {
            Condition       = condition;
            TrueExpression  = trueExpression;
            FalseExpression = falseExpression;

            MarkPosition(condition, falseExpression ?? trueExpression);
        }

        internal ConditionalExpression(SyntaxTreeNode parent, [NotNull] Expression trueExpression) {
            Parent = parent;
            bool invert = MaybeEat(TokenType.KeywordUnless);
            if (!invert) {
                Eat(TokenType.KeywordIf);
            }

            MarkStart(Token);

            TrueExpression = trueExpression;

            Condition = ParseOperation(this);
            if (MaybeEat(TokenType.KeywordElse)) {
                FalseExpression = ParseTestExpr(this);
            }

            if (invert) {
                (TrueExpression, FalseExpression) = (FalseExpression, TrueExpression);
            }
            
            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + TrueExpression + "if" + Condition;
            if (FalseExpression != null) {
                c = c + " else " + FalseExpression;
            }
            return c;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + Condition + "?" + TrueExpression + ":";
            if (FalseExpression == null) {
                c += "default";
            }
            else {
                c += FalseExpression;
            }

            return c;
        }
    }
}