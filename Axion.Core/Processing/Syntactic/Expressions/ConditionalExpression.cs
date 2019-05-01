using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         conditional_expr:
    ///             expr_list ('if' | 'unless') operation_expr ['else' expr_list];
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

        public override TypeName ValueType => TrueExpression.ValueType;

        /// <summary>
        ///     Constructs expression from tokens.
        /// </summary>
        internal ConditionalExpression(SyntaxTreeNode parent, Expression trueExpression) : base(
            parent
        ) {
            bool invert = MaybeEat(KeywordUnless);
            if (!invert) {
                Eat(KeywordIf);
            }

            MarkStart(Token);
            TrueExpression = trueExpression;
            Condition      = ParseOperation(this);
            if (MaybeEat(KeywordElse)) {
                FalseExpression = ParseMultiple(this, expectedTypes: Spec.PreGlobalExprs);
            }

            if (invert) {
                (TrueExpression, FalseExpression) = (FalseExpression, TrueExpression);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public ConditionalExpression(
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