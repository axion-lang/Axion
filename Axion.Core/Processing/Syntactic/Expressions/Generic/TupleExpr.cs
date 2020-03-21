using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic {
    /// <summary>
    ///     <c>
    ///         tuple-expr:
    ///             tuple-paren-expr | (multiple-expr [',']);
    ///         tuple-paren-expr:
    ///             '(' multiple-expr [','] ')';
    ///     </c>
    /// </summary>
    public class TupleExpr<T> : Multiple<T> where T : Expr {
        private NodeList<Expr> expressions;

        public NodeList<Expr> Expressions {
            get => expressions;
            set => expressions = BindNode(value);
        }

        [NoTraversePath]
        public override TypeName ValueType => new TupleTypeName(
            this, NodeList<TypeName>.From(
                this,
                Expressions.Select(e => e.ValueType)
            )
        );

        internal TupleExpr(
            Expr               parent,
            IEnumerable<Expr>? expressions = null
        ) : base(parent) {
            Expressions = NodeList<Expr>.From(this, expressions);
            if (Expressions.Count > 0) {
                MarkPosition(
                    Expressions[0],
                    Expressions[^1]
                );
            }
        }

        public TupleExpr<T> ParseEmpty() {
            Stream.Eat(OpenParenthesis);
            Stream.Eat(CloseParenthesis);
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }

        public override void ToPython(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }
    }
}