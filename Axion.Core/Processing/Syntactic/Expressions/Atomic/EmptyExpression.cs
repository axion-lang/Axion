using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     empty_expr:
    ///         ';' | 'pass'
    /// </summary>
    public class EmptyExpression : Expression {
        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        public EmptyExpression(AstNode parent) : base(parent) {
            MarkPosition(GetNext());
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("pass");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(";");
        }
    }
}