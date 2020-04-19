using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;

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

        [NoPathTraversing]
        public override TypeName ValueType => Literal.ValueType!;

        public static ConstantExpr True(Node parent) => new ConstantExpr(parent) {
            Literal = new Token(parent.Source, TokenType.KeywordTrue)
        };

        public static ConstantExpr False(Node parent) => new ConstantExpr(parent) {
            Literal = new Token(parent.Source, TokenType.KeywordFalse)
        };

        public static ConstantExpr Nil(Node parent) => new ConstantExpr(parent) {
            Literal = new Token(parent.Source, TokenType.KeywordNil)
        };

        public static ConstantExpr ParseNew(Expr parent) {
            return new ConstantExpr(parent).Parse();
        }

        public ConstantExpr(Node parent) : base(parent) { }

        public ConstantExpr Parse() {
            Literal = Stream.EatAny();
            return this;
        }
    }
}
