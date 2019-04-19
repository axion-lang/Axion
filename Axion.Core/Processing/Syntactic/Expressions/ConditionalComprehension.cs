using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parent  ('if' | 'unless') operation
    ///     </c>
    ///     Closes comprehensions tree,
    ///     cannot be continued with other comprehensions.
    /// </summary>
    public class ConditionalComprehension : Expression {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        internal override TypeName ValueType => Parent.ValueType;

        internal ConditionalComprehension(SyntaxTreeNode parent) : base(parent) {
            if (Peek.Is(TokenType.KeywordIf)) {
                MarkStart(TokenType.KeywordIf);
                Condition = ParseOperation(this);
            }
            else {
                MarkStart(TokenType.KeywordUnless);
                Condition = new UnaryOperationExpression(
                    this,
                    TokenType.KeywordNot,
                    ParseOperation(this)
                );
            }

            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(" if ", Condition);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}