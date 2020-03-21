using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         empty-expr:
    ///             ';' | 'pass';
    ///     </c>
    /// </summary>
    public class EmptyExpr : Expr {
        public Token Mark { get; set; }

        public EmptyExpr(
            Expr   parent,
            Token? mark = null
        ) : base(parent) {
            Mark = mark;
        }

        public EmptyExpr Parse() {
            SetSpan(() => { Mark = Stream.Eat(Semicolon, KeywordPass); });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Mark);
        }

        public override void ToCSharp(CodeWriter c) {
            // Don't write anything, semicolon is inserted at the scope level.
        }

        public override void ToPython(CodeWriter c) {
            c.Write("pass");
        }
    }
}