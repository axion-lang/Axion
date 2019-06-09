using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         conditional_comprehension:
    ///             ('if' | 'unless') infix_expr;
    ///     </c>
    ///     Closes comprehensions list,
    ///     cannot be continued with other comprehensions.
    /// </summary>
    public class ConditionalComprehension : Expression {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        public override TypeName ValueType => Parent.ValueType;

        /// <summary>
        ///     Constructs expression from tokens.
        /// </summary>
        internal ConditionalComprehension(AstNode parent) : base(parent) {
            if (Peek.Is(KeywordIf)) {
                MarkStartAndEat(KeywordIf);
                Condition = ParseInfixExpr(this);
            }
            else {
                MarkStartAndEat(KeywordUnless);
                Condition = new UnaryExpression(OpNot, ParseInfixExpr(this));
            }

            MarkEnd();
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        internal ConditionalComprehension(Expression condition) {
            Condition = condition;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(" if ", Condition);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotImplementedException();
        }
    }
}