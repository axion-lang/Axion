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
    }
}