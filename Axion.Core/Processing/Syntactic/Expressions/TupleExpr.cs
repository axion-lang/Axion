using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
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
        private NodeList<Expr> expressions = null!;

        public NodeList<Expr> Expressions {
            get => expressions;
            set {
                expressions = Bind(value);
                if (expressions.Count > 0) {
                    MarkPosition(
                        expressions[0],
                        expressions[^1]
                    );
                }
            }
        }

        [NoPathTraversing]
        public override TypeName ValueType => new TupleTypeName(
            this, NodeList<TypeName>.From(
                this,
                Expressions.Select(e => e.ValueType)
            )
        );

        internal TupleExpr(Expr parent) : base(parent) { }

        public TupleExpr ParseEmpty() {
            Stream.Eat(OpenParenthesis);
            Stream.Eat(CloseParenthesis);
            return this;
        }
    }
}