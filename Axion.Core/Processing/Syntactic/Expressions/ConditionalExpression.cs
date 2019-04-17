using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         cond_expr:
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

        public Expression TrueExpression {
            get => trueExpression;
            set => SetNode(ref trueExpression, value);
        }

        private Expression falseExpression;

        public Expression FalseExpression {
            get => falseExpression;
            set => SetNode(ref falseExpression, value);
        }

        internal override TypeName ValueType => TrueExpression.ValueType;

        public ConditionalExpression(
            Expression condition,
            Expression trueExpression,
            Expression falseExpression
        ) {
            Condition       = condition;
            TrueExpression  = trueExpression;
            FalseExpression = falseExpression;

            MarkPosition(condition, falseExpression ?? trueExpression);
        }

        internal ConditionalExpression(SyntaxTreeNode parent, Expression trueExpression) : base(
            parent
        ) {
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

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(TrueExpression, " if ", Condition);
            if (FalseExpression != null) {
                c.Write(" else ", FalseExpression);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Condition, " ? ", TrueExpression, " : ");
            if (FalseExpression == null) {
                c.Write("default");
            }
            else {
                c.Write(FalseExpression);
            }
        }
    }
}