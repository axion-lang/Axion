using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         const-expr:
    ///             CONST-TOKEN | STRING+;
    ///     </c>
    /// </summary>
    public class ConstantExpr : AtomExpr {
        private Token? literal;

        public Token? Literal {
            get => literal;
            set => literal = BindNullable(value);
        }

        public override TypeName? ValueType => Literal?.ValueType;

        public ConstantExpr(Node parent) : base(parent) { }

        public static ConstantExpr True(Node parent) {
            return new ConstantExpr(parent) {
                Literal = new Token(parent.Unit, TokenType.KeywordTrue)
            };
        }

        public static ConstantExpr False(Node parent) {
            return new ConstantExpr(parent) {
                Literal = new Token(parent.Unit, TokenType.KeywordFalse)
            };
        }

        public static ConstantExpr Nil(Node parent) {
            return new ConstantExpr(parent) {
                Literal = new Token(parent.Unit, TokenType.KeywordNil)
            };
        }

        public static ConstantExpr ParseNew(Node parent) {
            return new ConstantExpr(parent).Parse();
        }

        public ConstantExpr Parse() {
            Literal ??= Stream.EatAny();
            return this;
        }
    }
}
