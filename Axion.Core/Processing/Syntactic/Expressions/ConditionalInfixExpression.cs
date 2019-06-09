using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         conditional_expr:
    ///             expr_list ('if' | 'unless') infix_expr ['else' expr_list];
    ///     </c>
    /// </summary>
    public class ConditionalInfixExpression : Expression {
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

        public override TypeName ValueType => TrueExpression.ValueType;

        /// <summary>
        ///     Constructs expression from tokens.
        /// </summary>
        internal ConditionalInfixExpression(AstNode parent, Expression trueExpression) : base(
            parent
        ) {
            var invert = false;
            if (Peek.Is(KeywordIf)) {
                MarkStartAndEat(KeywordIf);
            }
            else {
                MarkStartAndEat(KeywordUnless);
                invert = true;
            }

            TrueExpression = trueExpression;
            Condition      = ParseInfixExpr(this);
            if (MaybeEat(KeywordElse)) {
                FalseExpression = ParseMultiple(this, expectedTypes: Spec.InfixExprs);
            }

            if (invert) {
                (TrueExpression, FalseExpression) = (FalseExpression, TrueExpression);
            }

            MarkEnd();
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public ConditionalInfixExpression(
            Expression condition,
            Expression trueExpression,
            Expression falseExpression
        ) {
            Condition       = condition;
            TrueExpression  = trueExpression;
            FalseExpression = falseExpression;
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