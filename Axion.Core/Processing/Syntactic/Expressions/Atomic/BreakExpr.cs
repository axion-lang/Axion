using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         break_expr:
    ///             'break' [name];
    ///     </c>
    /// </summary>
    public class BreakExpr : Expr, IStatementExpr {
        private NameExpr loopName;

        public NameExpr LoopName {
            get => loopName;
            set => SetNode(ref loopName, value);
        }

        public BreakExpr(
            Expr     parent   = null,
            NameExpr loopName = null
        ) : base(parent) {
            LoopName = loopName;
        }

        public BreakExpr Parse() {
            SetSpan(() => {
                Stream.Eat(KeywordBreak);
                if (Stream.PeekIs(Identifier)) {
                    LoopName = new NameExpr(this).Parse();
                }
            });

            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("break");
            if (LoopName != null) {
                c.Write(" ", LoopName);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("break");
        }

        public override void ToPython(CodeWriter c) {
            c.Write("break");
        }
    }
}