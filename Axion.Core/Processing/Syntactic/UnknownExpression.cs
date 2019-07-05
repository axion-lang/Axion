using System;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     <c>
    ///         unknown_expr:
    ///             TOKEN* (NEWLINE | END);
    ///     </c>
    /// </summary>
    public class UnknownExpression : Expression {
        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        public UnknownExpression(Expression parent) {
            Construct(parent, () => {
                while (!Peek.Is(Newline, End)) {
                    Eat();
                }
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}