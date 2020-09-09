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
        private Token? mark;

        public Token? Mark {
            get => mark;
            set => mark = BindNullable(value);
        }

        public EmptyExpr(Node parent) : base(parent) { }

        public EmptyExpr Parse() {
            Mark = Stream.Eat(Semicolon, KeywordPass);
            return this;
        }
    }
}
