using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic;

/// <summary>
///     <code>
///         const-expr:
///             CONST-TOKEN | STRING+;
///     </code>
/// </summary>
[Branch]
public partial class ConstantExpr : AtomExpr {
    [Leaf] Token? literal;

    public override TypeName? InferredType => Literal?.InferredType;

    public ConstantExpr(Node parent) : base(parent) { }

    public static ConstantExpr True(Node parent) {
        return new(parent) {
            Literal = new Token(parent.Unit, KeywordTrue)
        };
    }

    public static ConstantExpr False(Node parent) {
        return new(parent) {
            Literal = new Token(parent.Unit, KeywordFalse)
        };
    }

    public static ConstantExpr Nil(Node parent) {
        return new(parent) {
            Literal = new Token(parent.Unit, KeywordNil)
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
