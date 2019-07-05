using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         empty_expr:
    ///             ';' | 'pass';
    ///     </c>
    /// </summary>
    public class EmptyExpression : Expression {
        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        public EmptyExpression(Expression parent) {
            Construct(parent, () => { Eat(Semicolon, KeywordPass); });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("pass");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(";");
        }
    }
}