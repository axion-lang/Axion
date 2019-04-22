using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         parent  ('if' | 'unless') operation
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

        internal override TypeName ValueType => Parent.ValueType;

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ConditionalComprehension"/> from tokens.
        /// </summary>
        internal ConditionalComprehension(SyntaxTreeNode parent) : base(parent) {
            if (Peek.Is(KeywordIf)) {
                MarkStart(KeywordIf);
                Condition = ParseOperation(this);
            }
            else {
                MarkStart(KeywordUnless);
                Condition = new UnaryOperationExpression(OpNot, ParseOperation(this));
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="ConditionalComprehension"/> without position in source.
        /// </summary>
        internal ConditionalComprehension(Expression condition) {
            Condition = condition;
        }

        #endregion

        #region Transpilers

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(" if ", Condition);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            throw new NotImplementedException();
        }

        #endregion
    }
}