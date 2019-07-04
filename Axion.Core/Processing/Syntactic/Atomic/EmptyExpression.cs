using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     empty_expr:
    ///         ';' | 'pass'
    /// </summary>
    public class EmptyExpression : Expression {
        /// <summary>
        ///     Constructs from tokens.
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