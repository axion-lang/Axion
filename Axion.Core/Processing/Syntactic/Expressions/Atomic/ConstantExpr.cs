using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using Axion.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <code>
    ///         const-expr:
    ///             CONST-TOKEN | STRING+;
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class ConstantExpr : AtomExpr {
        [LeafSyntaxNode] Token? literal;

        public override TypeName? ValueType => Literal?.ValueType;

        public ConstantExpr(Node parent) : base(parent) { }

        public static ConstantExpr True(Node parent) {
            return new(parent) {
                Literal = new Token(parent.Unit, TokenType.KeywordTrue)
            };
        }

        public static ConstantExpr False(Node parent) {
            return new(parent) {
                Literal = new Token(parent.Unit, TokenType.KeywordFalse)
            };
        }

        public static ConstantExpr Nil(Node parent) {
            return new(parent) {
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
