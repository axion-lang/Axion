using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         tuple-expr:
    ///             tuple-paren-expr | (multiple-expr [',']);
    ///         tuple-paren-expr:
    ///             '(' multiple-expr [','] ')';
    ///     </c>
    /// </summary>
    public class TupleExpr : AtomExpr {
        private NodeList<Expr>? expressions;

        public NodeList<Expr> Expressions {
            get => InitIfNull(ref expressions);
            set => expressions = Bind(value);
        }

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
