using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         break-expr:
    ///             'break' [name];
    ///     </c>
    /// </summary>
    public class BreakExpr : Expr {
        private NameExpr? loopName;

        public NameExpr? LoopName {
            get => loopName;
            set => loopName = BindNullable(value);
        }

        public BreakExpr(Expr parent) : base(parent) { }

        public BreakExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordBreak);
                    if (Stream.PeekIs(Identifier)) {
                        LoopName = new NameExpr(this).Parse();
                    }
                }
            );

            return this;
        }
    }
}
