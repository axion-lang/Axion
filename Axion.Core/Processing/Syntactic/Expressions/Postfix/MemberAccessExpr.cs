using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         member-expr:
    ///             atom '.' ID;
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class MemberAccessExpr : PostfixExpr {
        [LeafSyntaxNode] Node? target;
        [LeafSyntaxNode] Node? member;

        public MemberAccessExpr(Node parent) : base(parent) { }

        public MemberAccessExpr Parse() {
            Target ??= AtomExpr.Parse(this);
            Stream.Eat(Dot);
            Member = AtomExpr.Parse(this);
            return this;
        }

        // TODO: check for accessing prop/field existence
    }
}
