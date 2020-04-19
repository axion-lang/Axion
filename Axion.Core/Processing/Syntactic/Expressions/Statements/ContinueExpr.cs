using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         continue-expr:
    ///             'continue' [name];
    ///     </c>
    /// </summary>
    public class ContinueExpr : Expr {
        private Token? kwContinue;

        public Token? KwContinue {
            get => kwContinue;
            set => kwContinue = BindNullable(value);
        }

        private NameExpr? loopName;

        public NameExpr? LoopName {
            get => loopName;
            set => loopName = BindNullable(value);
        }

        public ContinueExpr(Node parent) : base(parent) { }

        public ContinueExpr Parse() {
            KwContinue = Stream.Eat(KeywordContinue);
            if (Stream.PeekIs(Identifier)) {
                LoopName = new NameExpr(this).Parse();
            }
            return this;
        }
    }
}
