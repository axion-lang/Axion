using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <code>
    ///         tuple-expr:
    ///             tuple-paren-expr | (multiple-expr [',']);
    ///         tuple-paren-expr:
    ///             '(' multiple-expr [','] ')';
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class TupleExpr : AtomExpr {
        [LeafSyntaxNode] NodeList<Node>? expressions;

        public override TypeName ValueType =>
            new TupleTypeName(this) {
                Types = new NodeList<TypeName>(
                    this,
                    Expressions.Where(e => e.ValueType != null)
                        .Select(e => e.ValueType!)
                )
            };

        internal TupleExpr(Node parent) : base(parent) { }

        public TupleExpr ParseEmpty() {
            Stream.Eat(OpenParenthesis);
            Stream.Eat(CloseParenthesis);
            return this;
        }
    }
}
