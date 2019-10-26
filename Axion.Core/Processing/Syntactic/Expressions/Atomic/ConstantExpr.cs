using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         const_expr:
    ///             CONST_TOKEN | STRING+;
    ///     </c>
    /// </summary>
    public class ConstantExpr : Expr, IAtomExpr {
        public Token Literal { get; set; }

        public override TypeName ValueType => Literal.ValueType;

        public ConstantExpr(
            Expr  parent  = null,
            Token literal = null
        ) : base(parent) {
            Literal = literal;
        }

        public ConstantExpr(
            Expr   parent,
            string literal
        ) : base(parent) {
            Literal = new Token(Source, value: literal);
        }

        public ConstantExpr Parse() {
            SetSpan(() => { Literal = Stream.EatAny(); });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Literal);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(Literal);
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Literal);
        }
    }
}