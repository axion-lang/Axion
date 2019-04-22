using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         cond_expr:
    ///             expr ('if' | 'unless') operation ['else' expr]
    ///     </c>
    /// </summary>
    public class ConditionalExpression : Expression {
        #region Properties

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

        #endregion

        internal override TypeName ValueType => TrueExpression.ValueType;

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ConditionalExpression"/> from tokens.
        /// </summary>
        internal ConditionalExpression(SyntaxTreeNode parent, Expression trueExpression) : base(
            parent
        ) {
            bool invert = MaybeEat(TokenType.KeywordUnless);
            if (!invert) {
                Eat(TokenType.KeywordIf);
            }

            MarkStart(Token);
            TrueExpression = trueExpression;
            Condition      = ParseOperation(this);
            if (MaybeEat(TokenType.KeywordElse)) {
                FalseExpression = ParseTestExpr(this);
            }

            if (invert) {
                (TrueExpression, FalseExpression) = (FalseExpression, TrueExpression);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="ConditionalExpression"/> without position in source.
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

        #endregion

        #region Transpilers

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(TrueExpression, " if ", Condition);
            if (FalseExpression != null) {
                c.Write(" else ", FalseExpression);
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write(Condition, " ? ", TrueExpression, " : ");
            if (FalseExpression == null) {
                c.Write("default");
            }
            else {
                c.Write(FalseExpression);
            }
        }

        #endregion
    }
}