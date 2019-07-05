using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal ConditionalComprehension(Expression parent) {
            Construct(parent, () => {
                if (MaybeEat(KeywordIf)) {
                    Condition = ParseInfixExpr(this);
                }
                else {
                    Eat(KeywordUnless);
                    Condition = new UnaryExpression(this, new OperatorToken(OpNot), ParseInfixExpr(this));
                }
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(" if ", Condition);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotImplementedException();
        }
    }
}