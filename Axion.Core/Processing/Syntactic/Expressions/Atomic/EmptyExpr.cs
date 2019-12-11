using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         empty_expr:
    ///             ';' | 'pass';
    ///     </c>
    /// </summary>
    public class EmptyExpr : Expr, IAtomExpr, IStatementExpr {
        public Token Mark { get; set; }

        public EmptyExpr(
            Expr  parent = null,
            Token mark   = null
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

        public override void ToCSharp(CodeWriter c) { }

        public override void ToPython(CodeWriter c) {
            c.Write("pass");
        }
    }
}